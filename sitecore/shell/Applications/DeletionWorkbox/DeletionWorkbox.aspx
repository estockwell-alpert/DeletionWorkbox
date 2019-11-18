<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeletionWorkbox.aspx.cs" Inherits="DeletionWorkbox.DeletionWorkboxPage" %>

<!DOCTYPE html>

<style>
    table {
        width: 100%;
    }

    td.header {
        padding: 8px;
        color: white;
        background-color: #474747;
        font-family: sans-serif;
        font-size: 12px;
        font-weight: bold;
        letter-spacing: .3px;
    }

    .deleted-item span {
        display: block;
        margin-bottom:4px;
        font-size:14px;
    }

    span.itemName {
        font-weight: bold;
        font-family: sans-serif;
        font-size: 14px;
    }

    .deleted-item .box, .deleted-item > span {
    display: inline-block;
    vertical-align: top;
}

.deleted-item {
    margin: 10px 0;
    font-family: sans-serif;
}

.deleted-item .box {
    padding-left: 4px;
}

.deleted-item a {
    color: #2694C0;
    text-decoration: none;
    margin-top: 8px;
    display: inline-block;
    margin-right:20px;
    font-size: 13px;
}

span.databaseInfo {
    font-size: 14px;
}

.deleted-item a:hover {
    text-decoration: underline;
}

td.buttons {
    padding-top: 20px;
}

td.buttons a {
    text-decoration: none;
    font-family: sans-serif;
    color: black;
    font-size: 14px;
    display: inline-block;
    margin-bottom: 0;
    font-weight: normal;
    text-align: center;
    vertical-align: middle;
    cursor: pointer;
    border: 1px solid #bdbdbd;
    white-space: nowrap;
    padding: 8px 12px;
    font-size: 12px;
    line-height: 1.42857143;
    -webkit-user-select: none;
    -moz-user-select: none;
    -ms-user-select: none;
    user-select: none;
    min-width: 80px;
    text-shadow: none;
    outline: none;
    margin-left: 10px;
    background-repeat: repeat-x;
    -webkit-box-shadow: inset 0 1px #ffffff;
    box-shadow: inset 0 1px #ffffff;
    text-shadow: none;
    background-image: linear-gradient(to bottom, #f0f0f0 0%, #d9d9d9 100%);
    -moz-border-radius: 6px;
    -webkit-border-radius: 6px;
    border-radius: 6px;
    color: #131313;
}

span.item-id {
    display: inline-block;
    color: #bbb;
    font-size: 12px;
}

span.help {
    font-size: 13px;
    color: red;
}

td.buttons a:hover {
    text-decoration: underline;
}

table.targets {
    background: #fafafa;
   -webkit-box-shadow: 0 4px 4px -2px #000000;
   -moz-box-shadow: 0 4px 4px -2px #000000;
        box-shadow: 0 4px 4px -2px #000000;
}

table.targets.fixed{
    position:fixed;
    top:0;
    z-index:9;
}

.loading-modal {
    display: none;
    width: 100%;
    height: 100%;
    position: fixed;
    background: rgba(0,0,0,.2);
    top: 0;
    left: 0;
    z-index: 999;
}
.loading-box {
    position: absolute;
    top: 40%;
    padding: 40px;
    left: 42%;
    border-radius: 10px;
}
@keyframes spin {
    0% {
        transform: rotate(0deg);
    }
    100% {
        transform: rotate(360deg);
    }
}

.bottom-buttons a{
    margin-right:12px;
}

a#btnRefresh {
    color: #2694C0;
    text-decoration: none;
}

a#btnRefresh:hover {
    text-decoration: underline;
}
</style>

<link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css" />
<script src="https://code.jquery.com/jquery-2.2.4.min.js"></script>
<script src="https://code.jquery.com/ui/1.11.3/jquery-ui.min.js"></script>
<script type="text/javascript">

    $(document).ready(function () {
        $("a").on("click", function () {
            $(".loading-modal").show();
        });

        var top = $('table.targets').offset().top;

        $(window).scroll(function (event) {
            var y = $(this).scrollTop();
            console.log("y: " + y + ", top: " + top);
            if (y >= top) {
                $('table.targets').addClass('fixed');
                var height = $("table.targets").height();
                $("table.items").css("margin-top", height);
            }
            else {
                $('table.targets').removeClass('fixed');
                $("table.items").css("margin-top", "0");
            }
        });
    })

</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="font-family:sans-serif;margin:0">
    <form id="form1" runat="server">
        <asp:PlaceHolder runat="server" ID="phLoggedIn">
        <div class="loading-modal">
            <div class="loading-box">
                <img class="scSpinner" src="/sitecore/shell/themes/standard/Images/ProgressIndicator/sc-spinner32.gif" border="0" alt="" width="40px" />
            </div>
        </div>
        <div class="scWorkboxContentContainer">
            <table class="targets">
                <tbody>
                    <tr>
                        <td class="header">
                            <span>Target Databases</span>
                        </td>
                    </tr>
                    <asp:Repeater ID="rptTargetDatabases" runat="server" OnItemDataBound="rptTargetDatabases_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <asp:CheckBox ID="chkSelectTarget" runat="server" />
                                    <span class="database"><asp:Literal ID="litTarget" runat="server"></asp:Literal></span>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
            <table class="items">
                <tbody>
                    <tr>
                        <td class="header">
                            <span>Deleted Items</span>
                        </td>                        
                    </tr>
                    <tr>
                        <td style="position:absolute;right:0;padding:10px;">
                                                <asp:LinkButton ID="btnRefresh" runat="server" Text="Refresh" OnClick="btnRefresh_Click" />

                        </td>
                    </tr>
                    <asp:Repeater ID="rptItemList" OnItemDataBound="rptItemList_ItemDataBound" runat="server" OnItemCommand="rptItemList_ItemCommand">
                            <ItemTemplate>
                                <asp:PlaceHolder runat="server" ID="phItem">
                                    <tr>
                                        <td>
                                            <div class="deleted-item">
                                                <asp:CheckBox ID="chkDeleteItem" runat="server" />
                                                <div class="box">
                                                    <span class="itemName"><asp:Literal runat="server" ID="litName"></asp:Literal></span>
                                                    <span class="path"><asp:Literal ID="litPath" runat="server"></asp:Literal></span>
                                                    <span class="databaseInfo"><asp:Literal ID="litDatabaseInfo" runat="server"></asp:Literal></span>
                                                    <asp:LinkButton ID="btnDeleteItem" CommandName="DeleteItem" runat="server" Text="Delete"/>
                                                    <asp:LinkButton ID="btnRestoreItem" CommandName="RestoreItem" runat="server" Text="Restore"/>
                                                 </div>
                                            </div>
                                        </td>
                                    </tr>
                                </asp:PlaceHolder>
                            </ItemTemplate>  
                        </asp:Repeater>
                    <tr>
                        <td class="buttons">
                            <asp:LinkButton ID="btnDeleteSelected" runat="server" OnClick="btnDeleteSelected_Click">Delete (selected)</asp:LinkButton>
                            <asp:LinkButton ID="btnDeleteAll" runat="server" OnClick="btnDeleteAll_Click">Delete (all)</asp:LinkButton>
                            <asp:LinkButton ID="btnRestoreSelected" runat="server" OnClick="btnRestoreSelected_Click">Restore (selected)</asp:LinkButton>
                            <asp:LinkButton ID="btnRestoreAll" runat="server" OnClick="btnRestoreAll_Click">Restore (all)</asp:LinkButton>
                        </td>
                    </tr>
                </tbody>
            </table>
            <table style="margin-top:20px;">
                <tbody>
                    <tr>
                        <td class="header">Tools</td>
                    </tr>
                    <tr>
                        <td class="buttons">
                            <asp:LinkButton ID="btnScan" runat="server" Text="Initialize Workbox" OnClick="btnScan_Click"></asp:LinkButton>
                            <span style="margin-left:8px;font-size:13px;"> - Initialize the workbox by finding all items that exist in at least one publishing target but not master. This is usually only necessary after initial installation. THIS MAY TAKE A LONG TIME.</span>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder ID="phNotLoggedIn" runat="server" Visible="false">
            You must be logged in to view this page.
        </asp:PlaceHolder>
    </form>
</body>
</html>
