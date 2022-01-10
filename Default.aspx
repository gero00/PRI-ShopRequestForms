<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" type="text/css" href="StyleSheet.css" />
    <title></title>

</head>
<body>
    <!-- include element below is NOT a comment (even though it look like one). It is the element to include the common
         header elements stored in the HeaderInclude file -->
    <!--#include file="HeaderInclude.aspx"-->
    <form id="form1" runat="server">

        <div class="mainContainer">
            <h1>Service Request Dashboard</h1>
            <b>Service History Pages</b>
            <ul>
                <li><a class="landing" href="FacilitiesLookup.aspx">Facilities Lookup</a></li>
                <li><a class="landing" href="ShopLookup.aspx">Shop Lookup</a></li>
                <li><a class="landing" href="EquipmentMoveLookup.aspx">Equipment Move Lookup</a></li>
            </ul>
            <b>Customer Request Forms</b>
            <ul>
                <li><a class="landing" href="FacilitiesRequest.aspx">Facilities Request Form</a> </li>
                <li><a class="landing" href="ShopRequest.aspx">Shop Request Form</a></li>
                <li><a class="landing" href="EquipmentMove.aspx">Equipment Move Form</a> 
                </li>
            </ul>
            <br /><br />

        </div>
    </form>
</body>
</html>
