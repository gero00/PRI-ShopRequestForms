<%@ Page Language="VB" AutoEventWireup="false" CodeFile="EquipmentMove.aspx.vb" Inherits="EquipmentMove" MaintainScrollPositionOnPostback="True"%>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
    <title>Equipment Move Request</title>
</head>
<body>
    <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
         header elements stored in the HeaderInclude file -->
    <!--#include file="HeaderInclude.aspx"-->

     <div class="mainContainer">
        <!-- <div class="mainContent"> -->
            <form id="EquipmentMoveForm" runat="server">
                <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
                </asp:ToolkitScriptManager>  

                <asp:Button ID="ReturnToIntranet" CssClass="smallButton" runat="server" Text="Return to PRI Intranet" Visible ="True" />    
                <h1>Equipment Move Form</h1>
                <asp:Label ID="ErrorHeaderLabel" runat="server"></asp:Label>
                <asp:Label ID="SubmissionErrorLabel" runat="server"></asp:Label>
                <asp:Label ID="WONumberLabel" runat="server"></asp:Label>
                <asp:Label ID="RequestedOnLabel" runat="server"></asp:Label>
                <asp:Label ID="RequestedByLabel" runat="server"></asp:Label>
                <asp:Label ID="SurveyLabel" runat="server"></asp:Label>
                <asp:Label ID="PhoneLabel" runat="server"></asp:Label>
                <asp:Label ID="CompletionDateLabel" runat="server"></asp:Label>
                <asp:Label ID="NotesLabel" runat="server"></asp:Label>
                <asp:PlaceHolder ID="StaticTable" runat="server"></asp:PlaceHolder>
                <asp:Button CssClass="button" ID="RestartButton" runat="server" Text="Make Another Request" Visible="false" />
  

                <div id="FormEditableContent" runat="server">
                    <p>
                        To facilitate the Facilities crew moving your office/lab furniture and equipment, you must do two things:
                    </p>
                        <ul>
                            <li>generate a Survey-specific IT request to have your computer equipment moved (https://go.illinois.edu/prairieithelp) AND</li>
                            <li>complete and submit an electronic Equipment Move Form.</li>
                        </ul>

                    <p>
                        Note: <span style="color:red">Fields with red asterisks (**) are required</span>. Form will not submit if required field left blank.
                    </p>
                
                    <span style="color:red">**</span><b>Survey*</b><br />
                    <asp:DropDownList ID="SurveyChoice" runat="server">
                        <asp:ListItem Text="---Select Survey---" Value="0"></asp:ListItem>
                        <asp:ListItem Text="INHS" Value="1"></asp:ListItem>
                        <asp:ListItem Text="ISAS" Value="2"></asp:ListItem>
                        <asp:ListItem Text="ISGS" Value="3"></asp:ListItem>
                        <asp:ListItem Text="ISWS" Value="4"></asp:ListItem>
                        <asp:ListItem Text="ISTC" Value="5"></asp:ListItem>
                        <asp:ListItem Text="OED" Value="6"></asp:ListItem>
                    </asp:DropDownList>
                    <br /><br />

                    <!-- controls used to auto-complete the requestor textbox -->
                    <span style="color:red">**</span><b>Requested By</b><br />
                    <asp:TextBox runat="server" ID="RequestedBy" Columns="36" MaxLength="64"  
                        AutoPostBack="true" OnTextChanged="StaffAutocomplete_TextChanged" />
                    <asp:AutoCompleteExtender ID="AutoCompleteExtender1" TargetControlID="RequestedBy" UseContextKey="true" ServiceMethod="GetNames"
                        MinimumPrefixLength="3" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                        CompletionListCssClass="completionList" runat="server" >
                    </asp:AutoCompleteExtender>
                    <div class="finePrint">Enter last name, then wait to select your name from the dropdown</div>
                    <br />

                    <b>Phone</b><br />
                    <asp:TextBox ID ="Phone" runat="server"></asp:TextBox>
                    <div class="finePrint">Format: ###-###-####</div>

                    <p>
                        <span style="color:red">**</span><b>Requested Completion Date</b><br />
                        <asp:TextBox id="CompletionDate" runat="server"></asp:TextBox> <br />
                        <asp:CalendarExtender ID="CompletionDateExtender" runat="server" Format="yyyy-MM-dd" TargetControlID="CompletionDate" >
                        </asp:CalendarExtender>
                    </p>
                    <p>
                        <b>Additional Notes: </b><br />
                        <asp:TextBox TextMode="MultiLine" Rows="6" Columns="60" ID="Notes" runat="server"></asp:TextBox>
                    </p>
                    <p>
                        <span style="color:red">**</span><b>Item(s) To Be Moved</b><br />
                        Each item to be moved (including computer-related equipment) must be identified below.  The following fields are required for each item you need moved:
                            <ul>
                                <li><span style="color:red">**</span>All Rows must have 'Move Type' selected.</li>
                                <li><span style="color:red">**</span>All Rows must have a 'Description' filled in.</li>
                                <li>Items being Moved/Transferred (NOT trashed/surplused) require destination <span style="color:red">**</span>building, <span style="color:red">**</span>room, 
                                    and the <span style="color:red">**</span>name of the person responsible for the item after the move.</li>
                                <li><i>Please enter a tag number if one exists</i>. If your item has no tag number, enter a serial number. 
                                    If your item has no serial number, leave both fields blank.</li>
                            </ul>
                    </p>
                    <div>
                        <asp:Label ID="PTagMissing" runat="server"></asp:Label>
                        <asp:Label ID="ErrorHeaderItems" runat="server"></asp:Label>
                        <asp:Label ID="AddItemErrorLabel" runat="server"></asp:Label>
                    </div>
                    <div class="itemEntry">
                        <table>
                            <tr>
                                <th class="itemEntryHeader" runat="server">
                                    Move Type
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    From Building
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    From Room
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    To Building
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    To Room
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    Responsible Post-Move
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    PTag
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    Serial #
                                </th>
                                <th class="itemEntryHeader" runat="server">
                                    Description
                                </th>
                                <th runat="server">

                                </th>
                            </tr>
                            <tr>
                                <td>
                                    <asp:DropDownList ID="ddlMoveType" runat="server" AutoPostBack ="true" OnSelectedIndexChanged="MoveType_SelectedIndexChanged" Width="125px"></asp:DropDownList>
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlFromBuilding" runat="server" Width="200px" AutoPostBack="true" OnSelectedIndexChanged="FromBuilding_SelectedIndexChanged"></asp:DropDownList>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbFromRoom" runat="server" Width="70px"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:DropDownList ID="ddlToBuilding" runat="server" Width="200px"></asp:DropDownList>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbRoomNumber" runat="server" Width="60px"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="acCustodian" Columns="20" MaxLength="64"  
                                        AutoPostBack="true" OnTextChanged="CustodianAutocomplete_TextChanged" />
                                    <asp:AutoCompleteExtender ID="AutoCompleteExtender2" TargetControlID="acCustodian" UseContextKey="true" ServiceMethod="GetNames"
                                        MinimumPrefixLength="3" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                                        CompletionListCssClass="completionList" runat="server" >
                                    </asp:AutoCompleteExtender>
                                </td>
                                <td>
                                    <asp:TextBox runat="server" ID="acPTag" Columns="10" MaxLength="64"  AutoPostBack="true" OnTextChanged="PTag_TextChanged"/>
                                    <asp:AutoCompleteExtender ID="AutoCompleteExtender3" TargetControlID="acPtag" UseContextKey="true" ServiceMethod="GetPTag"
                                        MinimumPrefixLength="5" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                                        CompletionListCssClass="completionList" runat="server" >
                                    </asp:AutoCompleteExtender>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbSerialNumber" runat="server" Width="60px"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:TextBox ID="tbDescription" runat="server" Width="250px"></asp:TextBox>
                                </td>
                                <td>
                                    <asp:Button ButtonType="LinkButton" CssClass="smallButton" ID="AddItemButton" runat="server" text="Add Item"/>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <br />
                    <div>
                        <div id="MistakeInstructions" runat="server" visible="false"><i>Note: If you made an error in entry, please delete the incorrect row and re-add your item.</i></div>
                        <asp:GridView BorderStyle="None" id="RequestGrid" runat="server" AutoGenerateColumns="false" OnRowDeleting="OnDeleteRow" Width="1330px">  
                            <Columns>
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHeading" DataField="MoveType" HeaderText="Move Type" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="350px" HeaderStyle-CssClass="gvHeading" DataField="FromBuilding" HeaderText="From Building" />
                                <asp:BoundField ItemStyle-CssClass="gvHiddenColumn" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHiddenColumn" DataField="FromBuildingID" HeaderText="From Building ID"/>
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHeading" DataField="FromRoom" HeaderText="From Room" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="350px" HeaderStyle-CssClass="gvHeading" DataField="ToBuilding" HeaderText="To Building" />
                                <asp:BoundField ItemStyle-CssClass="gvHiddenColumn" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHiddenColumn" DataField="ToBuildingID" HeaderText="From Building ID"/>
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHeading" DataField="ToRoom" HeaderText="Room" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="250px" HeaderStyle-CssClass="gvHeading" DataField="Custodian" HeaderText="Custodian" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="50px" HeaderStyle-CssClass="gvHeading" DataField="PTag" HeaderText="PTag" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="50px" HeaderStyle-CssClass="gvHeading" DataField="SerialNumber" HeaderText="Serial#" />
                                <asp:BoundField ItemStyle-CssClass="gvField" ItemStyle-Width="420px" HeaderStyle-CssClass="gvHeading" DataField="Description" HeaderText="Description" />
                                <asp:CommandField ControlStyle-CssClass="smallButton" ItemStyle-CssClass="gvDeleteField" ItemStyle-Width="25px" HeaderStyle-CssClass="gvHeadingDeleteButton" ShowDeleteButton="true" ButtonType="Button" />
                            </Columns>  
                        </asp:GridView>
                    </div>
                    <asp:Button CssClass="button" ID="Submit" runat="server" Text="Submit Request" />
                </div>

            </form>
        <!-- </div> -->
    </div>



</body>
</html>

  
