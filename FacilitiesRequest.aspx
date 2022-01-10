<%@ Page Language="VB" Debug="true" AutoEventWireup="false" CodeFile="FacilitiesRequest.aspx.vb" Inherits="FacilitiesRequest" MaintainScrollPositionOnPostback="true" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
   <title>Facilities Request</title>

</head>
<body>
    <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
        header elements stored in the HeaderInclude file -->
    <!--#include file="HeaderInclude.aspx"-->

    <div class="mainContainer">

        <form id="FacilitiesRequestForm" runat="server">
            <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
            </asp:ToolkitScriptManager>  

            <asp:Button ID="ReturnToIntranet" CssClass="smallButton" runat="server" Text="Return to PRI Intranet" Visible ="True" />    
            <h1>Facilities Work Request</h1>

            <!-- used for user notification of success, errors for both previews and submissions -->
            <asp:Label ID="ResponseHeaderLabel" runat="server"></asp:Label>
            <asp:Label ID="BlankFieldsLabel" runat="server"></asp:Label>
            <asp:Label ID="WONumberLabel" runat="server"></asp:Label>
            <asp:Label ID="RequestedByLabel" runat="server"></asp:Label>
            <asp:Label ID="RequestedOnLabel" runat="server"></asp:Label>
            <asp:Label ID="BuildingLabel" runat="server"></asp:Label>
            <asp:Label ID="RoomLabel" runat="server"></asp:Label>
            <asp:Label ID="DescriptionLabel" runat="server"></asp:Label>
            <asp:Label ID="CompletionDateLabel" runat="server"></asp:Label>
            <asp:Button ID="RestartButton" CssClass="button" runat="server" Text="Make Another Request" Visible="false" />


            <div id="FullForm" runat="server">
                <p>If this issue is an emergency or urgent, please call Matt Thompson at 217-244-5006 prior to submitting the work request.</p>
                    <ul>
                        <li>To Facilitate getting a building issue repaired, please complete and submit this Facilities Work Request form.</li>
                        <li>Provide a detailed description of the issue.</li>
                        <li><span style="color:red">All fields are required.</span> Form will not submit if any field is left blank.</li>
                    </ul>

                <!-- controls for user data collection start here -->
                <div class="noPadding">
                    <span style="color:red">**</span><b>Requested by</b> <br />

                    <!-- controls used to auto-complete the requestor textbox -->                
                    <asp:TextBox runat="server" ID="RequestedBy" Columns="36" MaxLength="64"  AutoPostBack="true" OnTextChanged="RequestedByAutocomplete_TextChanged"/>
                    <asp:AutoCompleteExtender ID="AutoCompleteExtender1" TargetControlID="RequestedBy" UseContextKey="true" ServiceMethod="GetNames"
                        MinimumPrefixLength="3" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                        CompletionListCssClass="completionList" runat="server" >
                    </asp:AutoCompleteExtender>

                    <div class="finePrint">*Enter last name, then wait to select your name from the dropdown</div>
                </div>
                <p>
                    <!-- Building drop down, populated from database via code-behind -->
                    <span style="color:red">**</span><b>Building</b> <br />
                    <asp:DropDownList ID="BuildingChoices" runat="server">
                    </asp:DropDownList>
                </p>
                <p>
                    <span style="color:red">**</span><b>Room Number</b> <br />
                    <asp:TextBox ID="RoomNumber" runat="server" />
                </p>
                <p>
                    <!-- controls used to drop calendar in date field -->
                    <span style="color:red">**</span><b>Requested completion date</b> <br />
                    <asp:TextBox id="CompletionDate" runat="server"></asp:TextBox> <br />
                    <asp:CalendarExtender ID="CompletionDateExtender" runat="server" Format="yyyy-MM-dd" TargetControlID="CompletionDate" >
                    </asp:CalendarExtender>
                </p>
                <p> 
                    <span style="color:red">**</span><b>Description of work to be completed</b> <br />
                    <asp:TextBox TextMode="MultiLine" Rows="6" Columns="60" ID="Description" runat="server"></asp:TextBox>
                </p>
                <p>
                    <asp:Button CssClass="button" ID="SubmitButton" runat="server" text="Submit"/>
                </p>

            </div>

        </form>

    </div>
</body>
</html>

