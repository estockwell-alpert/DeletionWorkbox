<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DeletionWorkbox.aspx.cs" Inherits="DeletionWorkbox.DeletionWorkboxPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div class="scWorkboxContentContainer">
            <table>
                <tbody>
                    <tr>
                        <td class="header">
                            <span>Deleted Items</span>
                        </td>                        
                    </tr>
                    <asp:Repeater ID="rptItemList" OnItemDataBound="rptItemList_ItemDataBound" runat="server" OnItemCommand="rptItemList_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <div class="deleted-item">
                                            <asp:CheckBox ID="chkDeleteItem" runat="server" />
                                            <span class="itemName"><asp:Literal runat="server" ID="litName"></asp:Literal></span>
                                            <span class="databaseInfo"><asp:Literal ID="litDatabaseInfo" runat="server"></asp:Literal></span>
                                            <asp:LinkButton ID="btnDeleteItem" CommandName="DeleteItem" runat="server" />
                                        </div>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    <tr>
                        <td>
                            <asp:LinkButton ID="btnDeleteSelected" runat="server" OnClick="btnDeleteSelected_Click">Delete (selected)</asp:LinkButton>
                            <asp:LinkButton ID="btnDeleteAll" runat="server" OnClick="btnDeleteAll_Click">Delete (all)</asp:LinkButton>
                            <asp:LinkButton ID="btnRestoreSelected" runat="server" OnClick="btnRestoreSelected_Click">Restore (selected)</asp:LinkButton>
                            <asp:LinkButton ID="btnRestoreAll" runat="server" OnClick="btnRestoreAll_Click">Restore (all)</asp:LinkButton>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </form>
</body>
</html>
