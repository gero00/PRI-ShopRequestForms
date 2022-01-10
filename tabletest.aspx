<%@ Page Language="VB" AutoEventWireup="false" CodeFile="tabletest.aspx.vb" Inherits="Default2" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:GridView ID ="RequestGrid" runat="server" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="Row" HeaderText="Name" />
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <asp:TextBox ID="TB1" runat="server"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Age">
                        <ItemTemplate>
                            <asp:TextBox ID="TB2" runat="server"></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Drop It Down">
                        <ItemTemplate>
                            <asp:DropDownList ID="DDL" runat="server"></asp:DropDownList>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>


        <asp:Button ID="add" runat="server" text="Add Rows"/>
    </form>
</body>
</html>
