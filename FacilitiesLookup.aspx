<%@ Page Language="VB" AutoEventWireup="false" CodeFile="FacilitiesLookup.aspx.vb" Inherits="FacilitiesLookup" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
   <title>Facilities Request History</title>
</head>
<body>
    <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
         header elements stored in the HeaderInclude file -->
        <!--#include file="HeaderInclude.aspx"-->

     <div class="mainContainer">
        
        <form id="FacilitiesLookup" runat="server">

            <!-- required for specialty items to work -->
            <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
            </asp:ToolkitScriptManager>  

            <asp:Button ID="BackToDashboard" CssClass="smallButton" runat="server" Text="Back to Dashboard" Visible ="True" />    
            
            <h1>Facilities Request History</h1>

            <!-- Update confirmation only visible post update -->
            <div id="UpdateConfirmed" class="shadedSuccessBox" visible="false" runat="server">
                <h2>Request updated successfully</h2>
            </div>

            <!-- Gridview for displaying records as table on landing page. Always visible on first visit to facilities lookup site. -->
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
                            HeaderStyle-CssClass="gvHeadingDeleteButton" ShowSelectButton="true" SelectText="Details..." ButtonType="Button" />
                    </Columns>  
                </asp:GridView>
            </div>

            <!-- Only visible when record selected from main grid - controls for F&S to make updates to existing records. -->
            <div id="SingleRecord" runat="server" visible="false">
                <asp:Label ID="RequestID" runat="server" Visible="false"></asp:Label>
                <p>
                    <b>WO Number: </b> <asp:Label runat="server" ID="WONum"></asp:Label><br />
                    <b>Requested by:</b> <asp:Label runat="server" ID="RequestedBy"></asp:Label><br />
                    <b>Requested on:</b> <asp:Label runat="server" ID="RequestedOn"></asp:Label><br />
                    <b>Requested Completion Date:</b> <asp:Label id="RequestedCompletionDate" runat="server"></asp:Label> 
                </p>
                <p>
                    <b>Building:</b> <asp:Label ID="Building" runat="server"></asp:Label><br />
                    <b>Room:</b> <asp:Label ID="RoomNumber" runat="server"></asp:Label>
                </p>
                <p> 
                    <b>Description:</b> <br />
                    <asp:Label ID="Description" runat="server"></asp:Label>
                </p>
                <hr />
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
