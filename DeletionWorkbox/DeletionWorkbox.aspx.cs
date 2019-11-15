using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Net;
using System.IO;
using System.ServiceModel;
using System.Web.Configuration;
using Kofax.Library.LicenseService;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Collections;
using Sitecore.Data.Items;
using Sitecore.Data.Proxies;
using Sitecore.Exceptions;
using Sitecore.SecurityModel;
using Sitecore.Publishing;

namespace DeletionWorkbox
{
    public partial class DeletionWorkboxPage : System.Web.UI.Page
    {
        public Database db = Sitecore.Configuration.Factory.GetDatabase("master");
        public ItemList publishingTargets = new ItemList();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Sitecore.Context.User == null || Sitecore.Context.User.Name.ToLower() == "sitecore\\anonymous")
            {
                phLoggedIn.Visible = false;
                phNotLoggedIn.Visible = true;
            }

            publishingTargets = Sitecore.Publishing.PublishManager.GetPublishingTargets(db);

            if (!IsPostBack)
            {
                rptTargetDatabases.DataSource = publishingTargets;
                rptTargetDatabases.DataBind();

                SetWorkbox();  
            }
        }

        public void SetWorkbox()
        {
            //security
            var user = Sitecore.Context.User;

            var workbox = db.GetItem("/sitecore/system/Modules/Deletion Workbox/Workbox");
            if (workbox == null) return;

            var ids = workbox.Fields["Deleted Items"].Value.Split('|').Where(x => !String.IsNullOrEmpty(x));
            if (!ids.Any()) return;

            var items = new List<Item>();
            foreach (var id in ids)
            {
                if (db.GetItem(id) != null)
                {
                    // let's remove this from the saved settings
                    UpdateSavedState(id);
                    continue;
                }

                foreach (var target in publishingTargets)
                {
                    var targetDatabaseName = target["Target database"];
                    if (string.IsNullOrEmpty(targetDatabaseName))
                        continue;

                    var targetDatabase = Sitecore.Configuration.Factory.GetDatabase(targetDatabaseName);
                    if (targetDatabase == null)
                        continue;

                    var tempItem = targetDatabase.GetItem(id);
                    if (tempItem != null && !items.Any(x => x.ID.Equals(tempItem.ID)) && tempItem.Security.CanRead(user) && tempItem.Security.CanDelete(user))
                    {
                        items.Add(tempItem);
                        continue;
                    }
                }
            }

            items = items.OrderBy(x => x.Paths.Path).ToList();

            rptItemList.DataSource = items;
            rptItemList.DataBind();
        }

        protected void UpdateSavedState(string id)
        {
            var workbox = db.GetItem("/sitecore/system/Modules/Deletion Workbox/Workbox");
            if (workbox == null) return;

            using (new SecurityDisabler())
            {
                workbox.Editing.BeginEdit();
                workbox.Fields["Deleted Items"].Value = workbox.Fields["Deleted Items"].Value.ToLower().Replace(id.ToLower() + "|", "");
                workbox.Editing.EndEdit();
            }
        }

        protected void rptItemList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            Item item = e.Item.DataItem as Item;

            List<string> databases = new List<string>();

            foreach (var target in publishingTargets)
            {
                var targetDatabaseName = target["Target database"];
                if (string.IsNullOrEmpty(targetDatabaseName))
                    continue;

                var targetDatabase = Sitecore.Configuration.Factory.GetDatabase(targetDatabaseName);
                if (targetDatabase == null)
                    continue;

                var tempItem = targetDatabase.GetItem(item.ID.ToString());
                if (tempItem != null)
                {
                    databases.Add(targetDatabaseName);
                }
            }

            if (item == null)
            {
                PlaceHolder ph = e.Item.FindControl("phItem") as PlaceHolder;
                ph.Visible = false;
                return;
            }          

            CheckBox checkbox = e.Item.FindControl("chkDeleteItem") as CheckBox;
            Literal name = e.Item.FindControl("litName") as Literal;
            Literal path = e.Item.FindControl("litPath") as Literal;
            Literal info = e.Item.FindControl("litDatabaseInfo") as Literal;
            LinkButton button = e.Item.FindControl("btnDeleteItem") as LinkButton;

            //check if it's in recycle bin
            var inRecycleBin = CheckRecycleBin(item.ID.ToString());
            if (!inRecycleBin)
            {
                LinkButton restoreButton = e.Item.FindControl("btnRestoreItem") as LinkButton;
                restoreButton.Visible = false;
            }

            checkbox.Attributes["data-id"] = button.Attributes["data-id"] = item.ID.ToString();
            name.Text = item.Name;
            path.Text = item.Paths.FullPath + " - <span class='item-id'>" + item.ID.ToString() + "</span>";
            info.Text = "Databases: " + String.Join(",", databases);
        }

        protected List<Database> GetSelectedPublishingTargets()
        {
            List<Database> targets = new List<Database>();
            for (var i = 0; i < rptTargetDatabases.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptTargetDatabases.Items[i].FindControl("chkSelectTarget");
                if (chk.Checked)
                {
                    Literal name = (Literal)rptTargetDatabases.Items[i].FindControl("litTarget");
                    var database = Sitecore.Configuration.Factory.GetDatabase(name.Text);
                    targets.Add(database);
                }
            }
            return targets;
        }

        protected void rptItemList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var targets = GetSelectedPublishingTargets();

            LinkButton button = e.Item.FindControl("btnDeleteItem") as LinkButton;
            var id = button.Attributes["data-id"];

            if (e.CommandName == "DeleteItem")
            {
                DeleteItem(id);
            }
            else if (e.CommandName == "RestoreItem")
            {
                RestoreFromRecycleBin(id);
            }
            UpdateSavedState(id);
            SetWorkbox();
        }

        // we need to publish the deletion to trigger indexing and other on-publish actions
        protected void DeleteItem(string id)
        {
            // get from recycle bin and restore
            RestoreFromRecycleBin(id);

            // set Never Publish to true
            var item = db.GetItem(id);
            if (item == null) return;
            item.Editing.BeginEdit();
            item.Fields["__Never Publish"].Value = "1";
            item.Editing.EndEdit();

            // publish to all publishing targets
            foreach (var target in GetSelectedPublishingTargets())
            {
                PublishOptions po = new PublishOptions(db, target, PublishMode.SingleItem, Sitecore.Context.Language, DateTime.Now);
                po.RootItem = item;
                po.Deep = false;

                (new Publisher(po)).Publish();
            }

            item.Delete();
        }

        protected void RestoreFromRecycleBin(string id)
        {
            var archiveName = "recyclebin";
            var archive = Sitecore.Data.Archiving.ArchiveManager.GetArchive(archiveName, db);
            var itemId = new ID(id);
            var archivalId = archive.GetArchivalId(itemId);
            if (archivalId != Guid.Empty)
                archive.RestoreItem(archivalId);
        }

        protected bool CheckRecycleBin(string id)
        {
            var archiveName = "recyclebin";
            var archive = Sitecore.Data.Archiving.ArchiveManager.GetArchive(archiveName, db);
            var itemId = new ID(id);
            var archivalId = archive.GetArchivalId(itemId);
            return archivalId != Guid.Empty;
        }

        protected Item GetItemFromPublishingTarget(string id)
        {
            foreach (var target in publishingTargets)
            {
                var targetDatabaseName = target["Target database"];
                if (string.IsNullOrEmpty(targetDatabaseName))
                    continue;

                var targetDatabase = Sitecore.Configuration.Factory.GetDatabase(targetDatabaseName);
                if (targetDatabase == null)
                    continue;

                var tempItem = targetDatabase.GetItem(id);
                if (tempItem != null)
                {
                    return tempItem;
                }
            }
            return null;
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            var targets = GetSelectedPublishingTargets();
            for (var i = 0; i < rptItemList.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptItemList.Items[i].FindControl("chkDeleteItem");
                if (chk.Checked)
                {
                    var id = chk.Attributes["data-id"];
                    DeleteItem(id);
                    UpdateSavedState(id);
                }
            }           
            SetWorkbox();
        }

        protected void btnDeleteAll_Click(object sender, EventArgs e)
        {
            var targets = GetSelectedPublishingTargets();
            for (var i = 0; i < rptItemList.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptItemList.Items[i].FindControl("chkDeleteItem");
                var id = chk.Attributes["data-id"];
                DeleteItem(id);
                UpdateSavedState(id);
            }
            SetWorkbox();
        }

        protected void btnRestoreSelected_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < rptItemList.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptItemList.Items[i].FindControl("chkDeleteItem");
                if (chk.Checked)
                {
                    var id = chk.Attributes["data-id"];
                    RestoreFromRecycleBin(id);
                    UpdateSavedState(id);
                }
            }

            SetWorkbox();
        }

        protected void btnRestoreAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < rptItemList.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptItemList.Items[i].FindControl("chkDeleteItem");
                var id = chk.Attributes["data-id"];
                RestoreFromRecycleBin(id);
                UpdateSavedState(id);
            }

            SetWorkbox();
        }

        protected void rptTargetDatabases_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            Item target = e.Item.DataItem as Item;
       
            CheckBox checkbox = e.Item.FindControl("chkSelectTarget") as CheckBox;
            Literal name = e.Item.FindControl("litTarget") as Literal;

            checkbox.Checked = true;
            name.Text = target["Target database"];
        }
    }
}
