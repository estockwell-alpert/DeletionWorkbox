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

namespace DeletionWorkbox
{
    public partial class DeletionWorkboxPage : System.Web.UI.Page
    {
        public Database db = Sitecore.Configuration.Factory.GetDatabase("master");
        public ItemList publishingTargets;
        List<Item> workboxItems = new List<Item>();

        protected void Page_Load(object sender, EventArgs e)
        {
            var publishingTargets = Sitecore.Publishing.PublishManager.GetPublishingTargets(db);

            var workbox = db.GetItem("/sitecore/system/Modules/Deletion Workbox/Workbox");
            if (workbox == null) return;

            var ids = workbox.Fields["Deleted Items"].Value.Split('|').Where(x => !String.IsNullOrEmpty(x));
            if (!ids.Any()) return;

            rptItemList.DataSource = ids;
            rptItemList.DataBind();
        }

        protected void rptItemList_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            String id = e.Item.DataItem as String;

            // DO NOT ADD if this item exists in master
            if (db.GetItem(id) != null) return;

            List<string> databases = new List<string>();

            Item item = null;
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
                    item = tempItem;
                    databases.Add(targetDatabaseName);
                }
            }

            if (item == null) return;
            workboxItems.Add(item);

            CheckBox checkbox = e.Item.FindControl("chkDeleteItem") as CheckBox;
            Literal name = e.Item.FindControl("litName") as Literal;
            Literal info = e.Item.FindControl("litDatabaseInfo") as Literal;
            Button button = e.Item.FindControl("btnDeleteItem") as Button;

            checkbox.Attributes["data-id"] = button.Attributes["data-id"] = item.ID.ToString();
            name.Text = item.Name;
            info.Text = "Databases: " + String.Join(",", databases);
        }

        protected void rptItemList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "DeleteItem")
            {
                Button button = e.Item.FindControl("btnDeleteItem") as Button;
                var id = button.Attributes["data-id"];

                //ToDo: Phase 2 Target Database Selection
                foreach (var target in publishingTargets)
                {
                    var targetDatabaseName = target["Target database"];
                    if (string.IsNullOrEmpty(targetDatabaseName))
                        continue;

                    var targetDatabase = Sitecore.Configuration.Factory.GetDatabase(targetDatabaseName);
                    if (targetDatabase == null)
                        continue;

                    var item = targetDatabase.GetItem(id);
                    item.Delete();
                }
            }
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
            for (var i = 0; i < rptItemList.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptItemList.Items[i].FindControl("chkDeleteItem");
                if (chk.Checked)
                {
                    var id = chk.Attributes["data-id"];

                    // TO DO: ONLY DELETE FROM SELECTED PUBLISHING TARGETS
                    foreach (var target in publishingTargets)
                    {
                        DeleteItemFromTarget(id, target);
                    }
                }
            }
        }

        protected void btnDeleteAll_Click(object sender, EventArgs e)
        {
            // TO DO: ONLY DELETE FROM SELECTED PUBLISHING TARGETS
            foreach (var item in workboxItems)
            {
                var id = item.ID.ToString();
                foreach (var target in publishingTargets)
                {
                    DeleteItemFromTarget(id, target);
                }
            }
        }

        protected void DeleteItemFromTarget(string id, Item target)
        {
            var targetDatabaseName = target["Target database"];
            if (string.IsNullOrEmpty(targetDatabaseName))
                return;

            var targetDatabase = Sitecore.Configuration.Factory.GetDatabase(targetDatabaseName);
            if (targetDatabase == null)
                return;

            var tempItem = targetDatabase.GetItem(id);
            if (tempItem == null)
                return;

            tempItem.Delete();
        }

        protected void btnRestoreSelected_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < rptItemList.Items.Count; i++)
            {
                CheckBox chk = (CheckBox)rptItemList.Items[i].FindControl("chkDeleteItem");
                if (chk.Checked)
                {
                    var id = chk.Attributes["data-id"];
                    // find first occurance of this item in a database
                    // TO DO: SELECT SOURCE TO RESTORE FROM

                    var item = GetItemFromPublishingTarget(id);
                    if (item == null) continue;

                    RestoreItem(item);
                }
            }
        }

        protected void btnRestoreAll_Click(object sender, EventArgs e)
        {
            foreach (var item in workboxItems)
            {
                RestoreItem(item);
            }
        }

        protected void RestoreItem(Item item)
        {
            // To Do: SELECT SOURCE TO RESTORE FROM

            if (db.GetItem(item.ID.ToString()) != null) return;
            var newItem = CreateItem(item);
            Put(item, newItem, true);
        }

        public Item CreateItem(Item item)
        {
            var parentPath = item.Parent.Paths.Path;
            Item newItem = null;
            using (new SecurityDisabler())
            {
                var parent = db.GetItem(parentPath);
                if (parent == null)
                {
                    CreateItem(item.Parent);
                }
                else
                {
                    newItem = parent.Add(item.Name, item.Template);
                }
            }
            return newItem;
        }

        public void Put(Item source, Item destination, bool deep)
        {
            using (new ProxyDisabler())
            {
                ItemSerializerOptions defaultOptions = ItemSerializerOptions.GetDefaultOptions();
                defaultOptions.AllowDefaultValues = false;
                defaultOptions.AllowStandardValues = false;
                defaultOptions.ProcessChildren = deep;
                string outerXml = source.GetOuterXml(defaultOptions);
                try
                {
                    destination.Paste(outerXml, false, PasteMode.Overwrite);
                    Log.Audit(this, "Transfer from {0} to {1}. Deep: {2}", new[] { AuditFormatter.FormatItem(source), AuditFormatter.FormatItem(destination), deep.ToString() });
                }
                catch (TemplateNotFoundException)
                {
                    // Handle the template not found exception
                }
            }
        }
    }
}
