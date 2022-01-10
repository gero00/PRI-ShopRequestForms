<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ShopRequest.aspx.vb" Inherits="ShopRequest" MaintainScrollPositionOnPostback="true" %>
<%@ Register assembly="AjaxControlToolkit" namespace="AjaxControlToolkit" tagprefix="asp" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
    <title>Shop Services Request</title>
</head>
<body>
        <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
             header elements stored in the HeaderInclude file -->
        <!--#include file="HeaderInclude.aspx"-->
     <div class="mainContainer">

        <form id="ShopRequestForm" runat="server">

            <!-- required for specialty items to work - autofills, calendar element -->
            <asp:ToolkitScriptManager ID="ScriptManager1" runat="server">
            </asp:ToolkitScriptManager>

            <!-- allows user to leave PRI apps server and return to main intranet site on different server -->
            <asp:Button ID="ReturnToIntranet" CssClass="smallButton" runat="server" Text="Return to PRI Intranet" Visible ="True" />            

            
            <h1>Shop Services Work Request</h1>

            <!-- will only be displayed if preview button has been selected -->
            <!-- used for user notification of success, errors for both previews and submissions -->
            <asp:Label ID="PreviewHeaderLabel" runat="server"></asp:Label>
            <asp:Label ID="BlankFieldsLabel" runat="server"></asp:Label>
            <asp:Label ID ="WONumberLabel" runat="server"></asp:Label>
            <asp:Label ID="RequestedByLabel" runat="server"></asp:Label>
            <asp:Label ID="RequestedOnLabel" runat="server"></asp:Label>
            <asp:Label ID="CompletionDateLabel" runat="server"></asp:Label>
            <asp:Label ID="AccountInfoLabel" runat="server"></asp:Label>
            <asp:Label ID="AccountPILabel" runat="server"></asp:Label>
            <asp:Label ID="FundMgrLabel" runat="server"></asp:Label>
            <asp:Label ID="DescriptionLabel" runat="server"></asp:Label>
            <asp:Label ID="JustifyLabel" runat="server"></asp:Label>

            <asp:Button ID="RestartButton" CssClass="button" runat="server" Text="Make Another Request" Visible="false" />

            <div id="FullForm" class="formToggle" runat="server">

                    To facilitate getting equipment created/modified, repaired, or to have maintenance work performed, 
                        please complete and submit an electronic Shop Services Request form.
                <ul>

                    <li><span style="color:red">Fields with red asterisks (**) are required</span>. Form will not submit if required field left blank.</li>
                    <li>When your request is submitted, it will also be copied to your PI for review.</li>
                    <li>Provide a detailed description of the work to be completed. Send drawings in a separate email 
                        to Matt Thompson at <a class="dark" href="mailto: matt1966@illinois.edu">matt1966@illinois.edu</a> or 
                        Bob Bryant at <a class="dark" href="mailto: bryant2@illinois.edu">bryant2@illinois.edu</a></li>
                </ul>

                <!-- main form controls start here -->
                <div class="noPadding">
                        <span style="color:red">**</span><b>Requested by</b> <br />

                        <!-- controls used to auto-complete the requestor textbox -->                
                        <asp:TextBox runat="server" ID="RequestedBy" Columns="36" MaxLength="64"  AutoPostBack="true" OnTextChanged="StaffAutocomplete_TextChanged"/>
                        <asp:AutoCompleteExtender ID="AutoCompleteExtender1" TargetControlID="RequestedBy" UseContextKey="true" ServiceMethod="GetNames"
                            MinimumPrefixLength="3" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                            CompletionListCssClass="completionList" runat="server" >
                        </asp:AutoCompleteExtender>

                    <div class="finePrint">*Enter last name, then wait to select your name from the dropdown</div>
                </div>
                <br />
                <p>
                    <span style="color:red">**</span><b>Desired completion date</b> <br />

                    <!-- controls used to drop calendar for date field -->
                    <asp:TextBox id="CompletionDate" runat="server"></asp:TextBox> <br />
                    <asp:CalendarExtender ID="CompletionDateExtender" runat="server" Format="yyyy-MM-dd" TargetControlID="CompletionDate" >
                    </asp:CalendarExtender>
                </p>
                <span style="color:red">**</span><b>Account Information</b><br />
                <!-- controls used to auto-complete the account info textbox -->
                <asp:TextBox ID="txt_CFOP" runat="server" Width="500" AutoPostBack="true" OnTextChanged="txt_CFOP_TextChanged"></asp:TextBox>
                <asp:AutoCompleteExtender ID="AutoCompleteExtender3" TargetControlID="txt_CFOP" UseContextKey="true" ServiceMethod="SearchCfops"
                                    MinimumPrefixLength="12" EnableCaching="true"  CompletionSetCount="30" CompletionInterval="150" CompletionListCssClass="completionList" runat="server"  FirstRowSelected="true">
                </asp:AutoCompleteExtender>
                <br />
                <br />
                <div class="noPadding">
                        <span style="color:red">**</span><b>Account PI</b> <br />

                        <!-- controls used to auto-complete AccountPI field - though this shouldn't be required, because 
                             code behind fills this in based on account number -->
                        <asp:TextBox Enabled="false" CssClass="textboxNoEdit" runat="server" ID="AccountPI" Columns="40" MaxLength="64"  AutoPostBack="true" OnTextChanged="StaffAutocomplete_TextChanged"/>
                        <asp:AutoCompleteExtender ID="AutoCompleteExtender2" TargetControlID="AccountPI" UseContextKey="true" ServiceMethod="GetNames"
                            MinimumPrefixLength="3" EnableCaching="true"  CompletionSetCount="20" CompletionInterval="150" 
                            CompletionListCssClass="textboxNoEdit" runat="server" >
                        </asp:AutoCompleteExtender>
                        <br /><br />
                        <span style="color:red">**</span><b>Fund Manager</b> <br />
                        <asp:TextBox Enabled="false" CssClass="textboxNoEdit" runat="server" ID="FundMgr" Columns="40" MaxLength="64"></asp:TextBox>
                </div>
                <br />
                <p> 
                    <span style="color:red">**</span><b>Description of work to be completed</b> <br />
                    <asp:TextBox TextMode="MultiLine" Rows="6" Columns="60" ID="Description" runat="server"></asp:TextBox>
                </p>
                <p> 
                    <span style="color:red">**</span><b>Business justification for this request (including Who, What, When , Where, and Why/benefit to the University)</b> <br />
                    <asp:TextBox TextMode="MultiLine" Rows="6" Columns="60" ID="Justification" runat="server"></asp:TextBox>
                </p>
                <p>
                    <asp:Button ID="SubmitButton" CssClass="button" runat="server" text="Submit"/>
                </p>
            </div>
        </form>

    </div>

</body>
</html>
