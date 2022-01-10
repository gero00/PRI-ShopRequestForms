<%@ Page Language="VB" AutoEventWireup="false" CodeFile="EquipmentMoveLookup.aspx.vb" Inherits="Default2" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
   <title>Equipment Move Request History</title>
</head>
<body>
    <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
         header elements stored in the HeaderInclude file -->
        <!--#include file="HeaderInclude.aspx"-->

     <div class="mainContainer">
        <!-- <div class="mainContent"> -->
        <form id="EquipmentMoveLookup" runat="server">
            <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
            </asp:ToolkitScriptManager>  

            <asp:Button ID="BackToDashboard" CssClass="smallButton" runat="server" Text="Back to Dashboard" Visible ="True" />    
            <h1>Equipment Move Request History</h1>
            <div id="UpdateConfirmed" class="shadedSuccessBox" visible="false" runat="server">
                <h2>Request updated successfully</h2>
            </div>

            <div id="RecordList" runat="server">
                <asp:GridView BorderStyle="None" id="RecordGrid" runat="server" AutoGenerateColumns="false" Width="1045px" OnSelectedIndexChanged="RecordGrid_SelectedIndexChanged">  
                    <Columns>
                        <asp:BoundField ItemStyle-CssClass="gvHiddenColumn" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHiddenColumn" DataField="ID" HeaderText="ID"/>
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="70px" HeaderStyle-CssClass="lookupHeading" ItemStyle-HorizontalAlign="Center" 
                            HeaderStyle-HorizontalAlign="Center" DataField="WONumber" HeaderText="WO#" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="125px" HeaderStyle-CssClass="lookupHeading" ItemStyle-HorizontalAlign="Center" 
                            HeaderStyle-HorizontalAlign="Center" DataField="RequestDate" HeaderText="Requested On" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="70px" HeaderStyle-CssClass="lookupHeading" ItemStyle-HorizontalAlign="Center" 
                            HeaderStyle-HorizontalAlign="Center" DataField="Survey" HeaderText="Survey" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="250px" HeaderStyle-CssClass="lookupHeading" DataField="RequestedBy" HeaderText="Requestor" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="125px" 
                            HeaderStyle-CssClass="lookupHeading" HeaderStyle-HorizontalAlign="Center" 
                            DataField="RequestedCompletionDate" HeaderText="Req Comp Date" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="100px" HeaderStyle-CssClass="lookupHeading" 
                            ItemStyle-HorizontalAlign="Center" HeaderStyle-HorizontalAlign="Center"  DataField="ServiceStatus" HeaderText="Status" />
                        <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="150px" 
                            HeaderStyle-CssClass="lookupHeading" HeaderStyle-HorizontalAlign="Center" 
                            DataField="ActualCompletionDate" HeaderText="Act Comp Date" />
                        <asp:CommandField ControlStyle-CssClass="smallButton" ItemStyle-CssClass="gvField" ItemStyle-Width="25px" 
                            HeaderStyle-CssClass="gvHeadingDeleteButton" ShowSelectButton="true" SelectText="Details..." ButtonType="Button" />
                    </Columns>  
                </asp:GridView>
            </div>
            <div id="SingleRecord" runat="server" visible="false">
                <asp:Label ID="RequestID" runat="server" Visible="false"></asp:Label>
                <div id="Column1" runat="server" class="lookupColumn1">
                    <p>
                        <b>WO Number:</b> <asp:Label runat="server" ID="WONumber"></asp:Label><br />
                        <b>Requested On:</b> <asp:Label runat="server" ID="RequestDate"></asp:Label><br />
                        <b>Survey:</b> <asp:Label runat="server" ID="Survey"></asp:Label><br />
                        <b>Requested by:</b> <asp:Label runat="server" ID="RequestedBy"></asp:Label>(<asp:Label runat="server" ID="Phone"></asp:Label>)<br />
                        <b>Requested Completion Date:</b> <asp:Label id="RequestedCompletionDate" runat="server"></asp:Label> 
                    </p>
                    <p> 
                        <b>Requestor Notes:</b> <br />
                        <asp:Label ID="RequestorNotes" runat="server"></asp:Label>
                    </p>
                </div>
                <div id="Column2" runat="server" class="lookupColumn2">
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
                        <asp:TextBox ID="tbServiceNotes" TextMode="MultiLine" runat="server" width="480px" Height="200px"></asp:TextBox><br />
                        <asp:Button CssClass="button" ID="UpdateRecord" runat="server" Text="Update Service Record" />
                        <asp:Button CssClass="button" ID ="ReturnToList" runat="server" Text="Return to Request History List" />
                    </p>
                </div>
                <div id="ItemList" runat="server">
                    <p><br />
                        <asp:GridView BorderStyle="None" id="ItemGrid" runat="server" AutoGenerateColumns="false" Width="1550px" OnSelectedIndexChanged="RecordGrid_SelectedIndexChanged">  
                            <Columns>
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="25px" HeaderStyle-CssClass="lookupHeading" DataField="MoveType" HeaderText="Move Type" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="300px" HeaderStyle-CssClass="lookupHeading" DataField="FromBuilding" HeaderText="From Building" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="100px" HeaderStyle-CssClass="lookupHeading" DataField="FromRoom" HeaderText="FromRoom" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="300px" HeaderStyle-CssClass="lookupHeading" DataField="ToBuilding" HeaderText="To Building" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="100px" HeaderStyle-CssClass="lookupHeading" DataField="ToRoom" HeaderText="Room" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="200px" HeaderStyle-CssClass="lookupHeading" DataField="Custodian" HeaderText="Custodian" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Center" 
                                    HeaderStyle-CssClass="lookupHeading" HeaderStyle-HorizontalAlign="Center" DataField="PTag" HeaderText="PTag" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-HorizontalAlign="Center" ItemStyle-Width="75px" 
                                    HeaderStyle-CssClass="lookupHeading" HeaderStyle-HorizontalAlign="Center" 
                                    DataField="SerialNumber" HeaderText="Serial#" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="400px" HeaderStyle-CssClass="lookupHeading" 
                                    DataField="Description" HeaderText="Description" />
                            </Columns>  
                        </asp:GridView>
                    </p>
                </div>
        </div>
        </form>
     </div>
</body>
</html>
