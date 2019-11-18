# DeletionWorkbox
Sitecore Module to help users publish deletions to web databases

This module is intended to help in the instances where a user has deleted an item from master without first removing it from web, and publishing the parent item is not an option (e.g. if the parent item is the site root). This module will help users publish deletions to web without switching to the web database and modifying content directly in web. Users can also choose to restore items from the deletion workbox (rather than finding themin the recycle bin). 

The Deletion Workbox also provides users with a visual interface that can show them all of the items that exist in web (or any other publishing target) but no longer exist in master. This can help users to view and keep track of all unpublished deletions.

# Installation
User the Installation Wizard to install the .zip package (available in the latest Release). When prompted, choose <b>merge</b> for Sitecore items and <b>overwrite</b> for files. 
  
# Set Up
Once the Workbox is installed, you can click Initialize Workbox to get all items that have been deleted from master but still exist in at least one publishing target. After you've done this once, you should not have to do it again, as the custom pipeline will automatically add items as they are deleted once the module is installed. This is optional and only needed if you want to add items to the workbox that were deleted prior to installation.

# Dependencies
This module has been tested in Sitecore 8.2 and 9.0. It has not been tested in earlier versions of Sitecore but may work.

# To Use
You must be logged into Sitecore to access the tool.
Access the tool in the Sitecore Start menu (right side) or at [your site]/sitecore/shell/applications/deletionworkbox/deletionworkbox.aspx

# Security
Items will only be available in the workbox if the current user has access to view and delete them. 

# Files included in Package:
\bin\DeletionWorkbox.dll

\App_Config\Include\DeletionWorkboxModule\DeletionWorkboxPipeline.config

\sitecore\shell\Applications\DeletionWorkbox\DeletionWorkbox.aspx

/temp/IconCache/office/16x16/remove_version.png

/temp/IconCache/office/24x24/remove_version.png

/temp/IconCache/office/32x32/remove_version.png

/sitecore/shell/Themes/Standard/Images/ProgressIndicator/sc-spinner32.gif

core:/sitecore/content/Applications/Deletion Workbox

core:/sitecore/content/Documents and settings/All users/Start menu/Right/Deletion Publishing Workbox

master:/sitecore/templates/Modules/Deletion Workbox Module/Workbox

master:/sitecore/system/Modules/Deletion Workbox/Workbox
