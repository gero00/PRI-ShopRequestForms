<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ShopLookup.aspx.vb" Inherits="ShopLookup" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
   <title>Shop Request History</title>
</head>
<body>
    <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
         header elements stored in the HeaderInclude file -->
    <!--#include file="HeaderInclude.aspx"-->

     <div class="mainContainer">
        
        <form id="ShopLookup" runat="server">

            <!-- required for specialty items to work -->
            <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
            </asp:ToolkitScriptManager>  

            <asp:Button ID="BackToDashboard" CssClass="smallButton" runat="server" Text="Back to Dashboard" Visible ="True" />      
            <h1>Shop Request History</h1>

            <!-- Update confirmation only visible post update -->
            <div id="UpdateConfirmed" class="shadedSuccessBox" visible="false" runat="server">
                <h2>Request updated successfully</h2>
            </div>

            <!-- Gridview for displaying records as table on landing page. Always visible on first visit to shop lookup site. -->
            <div id="RecordList" runat="server">
                <asp:GridView BorderStyle="None" id="RecordGrid" runat="server" AutoGenerateColumns="false" Width="1045px" 
                    OnSelectedIndexChanged="RecordGrid_SelectedIndexChanged" OnPageIndexChanging="RecordGrid_PageIndexChanging" 
                    PagerStyle-CssClass="gvPager" PagerStyle-HorizontalAlign="Center" 
                    AllowPaging="true" PagerSettings-Mode="NumericFirstLast" PageSize="20">  
                    <Columns>
                        <asp:BoundField ItemStyle-CssClass="gvHiddenColumn" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHiddenColumn" DataField="ID" HeaderText="ID"/>
                        <asp:BoundField ItemStyle-CssClass="gvWOField" ItemStyle-Width="35px" HeaderStyle-CssClass="lookupHeading" DataField="WONumber" HeaderText ="WO#" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="250px" HeaderStyle-CssClass="lookupHeading" DataField="RequestedBy" HeaderText="Requestor" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="125px" HeaderStyle-CssClass="lookupHeading"
                            HeaderStyle-HorizontalAlign="Center" DataField="RequestDate" HeaderText="Requested On" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="125px" 
                            HeaderStyle-CssClass="lookupHeading" HeaderStyle-HorizontalAlign="Center" 
                            DataField="RequestedCompletionDate" HeaderText="Needed By" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center" 
                            HeaderStyle-HorizontalAlign="Center" HeaderStyle-CSSClass="lookupHeading" DataField="ServiceStatus" HeaderText="Job Status" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="125px" 
                            HeaderStyle-CssClass="lookupHeading" HeaderStyle-HorizontalAlign="Center" 
                            DataField="ActualCompletionDate" HeaderText="Act Comp Date" />                        
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="500px" HeaderStyle-CssClass="lookupHeading" DataField="Description" HeaderText="Description" />
                        <asp:CommandField ControlStyle-CssClass="smallButton" ItemStyle-CssClass="gvField" ItemStyle-Width="25px" 
                            HeaderStyle-CssClass="lookupHeading" ShowSelectButton="true" SelectText="Details..." ButtonType="Button" />
                    </Columns>  
                </asp:GridView>
            </div>

            <!-- Only visible when record selected from main grid - controls for F&S to make updates to existing records. -->
            <div id="SingleRecord" runat="server" visible="false">
                <asp:Label ID="RequestID" runat="server" Visible="false"></asp:Label>
                <p>
                    <b>WO Number:</b> <asp:Label runat="server" ID="WONumber"></asp:Label><br />
                    <b>Requested By:</b> <asp:Label runat="server" ID="RequestedBy"></asp:Label><br />
                    <b>Requested On:</b> <asp:Label runat="server" ID="RequestedOn"></asp:Label><br />
                    <b>Requested Completion Date:</b> <asp:Label id="RequestedCompletionDate" runat="server"></asp:Label> 
                </p>
                <p> 
                    <b>Description:</b> <br />
                    <asp:Label ID="Description" runat="server"></asp:Label>
                </p>
                <p>
                <p> 
                    <b>Business justification:</b> <br />
                    <asp:Label ID="Justification" runat="server"></asp:Label>
                </p>
                </p>
                <hr />
                <p>
                <b>Account Information</b><br />
                    <asp:TextBox ID="txt_CFOP" runat="server" Width="500" AutoPostBack="true" OnTextChanged=" txt_CFOP_TextChanged"></asp:TextBox>
                    <asp:AutoCompleteExtender ID="AutoCompleteExtender3" TargetControlID="txt_CFOP" UseContextKey="true" ServiceMethod="SearchCfops"
                        MinimumPrefixLength="12" EnableCaching="true"  CompletionSetCount="30" CompletionInterval="150" CompletionListCssClass="completionList" 
                        runat="server"  FirstRowSelected="true">
                    </asp:AutoCompleteExtender>
                </p>
                <p>
                    <b>Account PI</b> <br />
            
                    <asp:TextBox runat="server" ID="PI" Columns="40" MaxLength="64"  AutoPostBack="true" OnTextChanged="StaffAutocomplete_TextChanged"/>
                    <asp:AutoCompleteExtender ID="AutoCompleteExtender2" TargetControlID="PI" UseContextKey="true" ServiceMethod="GetNames"
                        MinimumPrefixLength="3" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                        CompletionListCssClass="completionList" runat="server" >
                    </asp:AutoCompleteExtender>

                </p>
                <p>
                    <b>Fund Manager</b> <br />
                    <asp:TextBox runat="server" ID="FundMgr" Columns="40" MaxLength="64"></asp:TextBox>
                </p>
                <p>
                    <b>Status:</b> <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack ="true" OnSelectedIndexChanged="ddlStatus_SelectedIndexChanged"></asp:DropDownList>
                </p>
                <p id="CompletionDateSection" runat="server" visible="false">
                    <b>Completion Date: </b>
                    <asp:TextBox id="CompletionDate" runat="server" ></asp:TextBox> <br />
                    <asp:CalendarExtender ID="CompletionDateExtender" runat="server" Format="yyyy-MM-dd" TargetControlID="CompletionDate" >
                    </asp:CalendarExtender>
                </p>
                <p>
                    <b>Service Notes:</b> <br />
                    <asp:TextBox ID="tbServiceNotes" TextMode="MultiLine" runat="server" width="600px" Height="400px"></asp:TextBox><br />
                    <asp:Button CssClass="button" ID="UpdateRecord" runat="server" Text="Update Service Record" />
                    <asp:Button CssClass="button" ID ="ReturnToList" runat="server" Text="Return to Request History List" />
                </p>
        </div>
        </form>
     </div>
</body>
</html>
