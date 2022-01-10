<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ErrorPage.aspx.vb" Inherits="ErrorPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
   <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
   <title>F&S Form Error</title>
</head>
<body>
    <div class="mainContainerErrorPage">
        <form id="form1" runat="server">
            <h1>Error Submitting Form</h1>
            <p class="errorInstructions">Please email itsupport@isws.illinois.edu with a screenshot of the error below and the
                information you were submitting via this form.
            </p>
            <div class="shadedSysErrorBox">
                <asp:Label ID="errorMessage" runat="server"></asp:Label>
            </div>
        </form>
    </div>

</body>
</html>
