using Sitecore.Web.UI.Sheer;
using System;

namespace DeletionWorkbox
{
    public class DeletionWorkboxHandler
    {
        public void Process(ClientPipelineArgs args)
        {

            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                try
                {
                    string databaseName = args.Parameters["database"];
                    var ids = args.Parameters["items"];

                    var db = Sitecore.Configuration.Factory.GetDatabase("master");

                    if (String.IsNullOrEmpty(ids) || String.IsNullOrEmpty(databaseName)) return;

                    var item = db.GetItem(ids);
                    var path = item.Paths.Path.ToLower();

                    if (!path.StartsWith("/sitecore/content/")) return;

                    if (databaseName.ToLower() == "master")
                    {
                        var workbox = db.GetItem("/sitecore/system/Modules/Deletion Workbox/Workbox");
                        if (workbox == null) return;
                        workbox.Editing.BeginEdit();
                        workbox.Fields["Deleted Items"].Value += ids + "|";
                        workbox.Editing.EndEdit();
                    }

                }
                catch (Exception ex)
                {
                    Sitecore.Diagnostics.Log.Error("Failed to retrieve ID of deleted item", this);
                }

            }
        }
    }
}