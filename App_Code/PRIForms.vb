Imports System.Data
Imports System.Data.SqlClient


Public Class PRIForms

    'variables to hold lists of names for auto-complete element RequestedBy
    Public Shared staff_names As New List(Of String)()
    Public Shared staff_uins As New List(Of String)()
    Public Shared FundManager As String
    Public Shared FMEmail As String

    Public Shared errorMessageContent As String = ""

    'Used by all forms that have drop-downs with PRI building names to populate those controls
    Public Shared Sub PopulateBuildingList(ByRef BuildingChoices As DropDownList)
        Try
            Dim ConnString As String = ""
            Dim Schema As String = ""

            'only use production database if code is running on production server
            If PRIForms.getHost() = "apps.prairie.illinois.edu" Then
                ConnString = ConfigurationManager.ConnectionStrings("DatastormConnStr").ConnectionString
                Schema = "SPACE"
            Else
                ConnString = ConfigurationManager.ConnectionStrings("BldgConnStr").ConnectionString
                Schema = "Archibus"
            End If

            'get info for populating list and bind it to the control
            Using conn As New SqlConnection(ConnString)
                Using cmd As New SqlCommand("Select BLDG_ID, DISPLAY_NAME FROM " & Schema & ".buildings_NP1 order by DISPLAY_NAME")
                    cmd.CommandType = CommandType.Text
                    cmd.Connection = conn
                    conn.Open()
                    BuildingChoices.DataSource = cmd.ExecuteReader()
                    BuildingChoices.DataTextField = "DISPLAY_NAME"
                    BuildingChoices.DataValueField = "BLDG_ID"
                    BuildingChoices.DataBind()
                    conn.Close()
                End Using
            End Using

            'add a non-building item at 0 level to instruct user
            BuildingChoices.Items.Insert(0, New ListItem("--Select Building--", "0"))

        Catch ex As Exception
            HttpContext.Current.Response.Write(ex.ToString())

        End Try
    End Sub

    'Cleans building string to remove a lot of extraneous information for ease of reading
    Public Shared Function GetCleanBuildingString(ByVal buildingString As String) As String

        Dim splitStrArray As String()
        Dim beginBldgStr As String = ""
        Dim endBldgStr As String = ""
        Dim cleanBldgStr As String = ""

        If buildingString <> "Surplus" And buildingString <> "To Trash/Surplus" Then
            splitStrArray = buildingString.Split("(")
            beginBldgStr = splitStrArray(0)

            buildingString = buildingString.Replace("UIUC Bldg", "$")
            splitStrArray = buildingString.Split("$")
            endBldgStr = " - UIUC Bldg" + splitStrArray(1)

            cleanBldgStr = beginBldgStr + endBldgStr
        Else
            cleanBldgStr = buildingString
        End If

        Return cleanBldgStr
    End Function

    'In partnership with txt_RequestorName_TextChanged function, auto-completes requestor box to validate against university directory
    <System.Web.Script.Services.ScriptMethod()>
    <System.Web.Services.WebMethod()>
    Public Shared Function GetNames(ByVal prefixText As String, ByVal count As Integer) As List(Of String)

        'must clear these variables Or they duplicate previously selected items in dropdown
        staff_names.Clear()
        staff_uins.Clear()

        'The connection info for for connecting to the database where the staff names are
        Try

            'Dim RequestorConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString
            Dim RequestorConnStr As String = ConfigurationManager.ConnectionStrings("DatastormFiscalConnStr").ConnectionString
            Dim ReqConn As New SqlConnection(RequestorConnStr)
            Dim cmd As New SqlCommand
            Dim dataReader As SqlDataReader
            Dim staff_name As String = ""

            ReqConn.Open()
            Dim sql_get_staffname As String = "select Name, UINNoDash from vwStaffNamePurchRequestInfo where" & " Name Like '" & prefixText & "%';"

            cmd = New SqlCommand(sql_get_staffname, ReqConn)
            dataReader = cmd.ExecuteReader()

            'add all items returned from the database to the autocomplete element
            While dataReader.Read()
                staff_name = "staff_name - " & dataReader.Item(0)
                staff_names.Add(dataReader.Item(0))
                staff_uins.Add(dataReader.Item(1))
            End While
            dataReader.Close()
            cmd.Dispose()

            Return staff_names

        Catch ex As Exception
            Return staff_names
        End Try


    End Function

    'In partnership with getnames, autocompletes requestor box to validate against university directory
    Public Shared Sub StaffAutocomplete_TextChanged(ByVal sender As Object, ByVal e As EventArgs,
                                                    ByRef txtBox As List(Of TextBox), ByRef uinLabel As Label)
        Try
            Dim staff_name_index As Integer
            Dim staff_txtbox As TextBox = sender
            Dim form_txtbox As List(Of TextBox) = txtBox
            Dim staff_uin_lbl As Label = uinLabel
            Dim staff_name_txt As String = staff_txtbox.Text()

            'if there is content in staff_name_text, get its list index from the staff_names list
            'get the user's UIN from the staff_ using the staff_name_index for the staff_uins list
            'at time of development, code does not use UIN but is set to retrieve it for potential future needs
            If staff_name_txt <> Nothing And staff_name_txt <> "" Then
                staff_name_index = staff_names.IndexOf(staff_name_txt)
                'If staff_name_index <> Nothing Then
                'End If

                If staff_name_index >= 0 Then
                    If staff_uins(staff_name_index) <> Nothing And staff_uins(staff_name_index) <> "" Then
                        'staff_uin_lbl.Text = staff_uins(staff_name_index)
                    Else
                        For Each textbox In form_txtbox
                            textbox.Text = ""
                        Next
                    End If
                Else
                    For Each textbox In form_txtbox
                        textbox.Text = ""
                    Next
                End If
            Else
                MsgBox("A recognized staff name is required. Please select a name from the generated staff listing in the dropdown list using last name first.")
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.Print("Error acquiring Staff Requestor UIN: " & ex.ToString())
        End Try

    End Sub

    'For all forms' date fields - verifies to see if date user selected as "Requested Completion Date"
    'is a date in the future, since F&S doesn't have a budget for time machines
    Public Shared Function checkDates(DateString As String) As Boolean

        Dim DateCheck As Boolean = True

        If DateString < Today() Then
            DateCheck = False
        End If

        Return DateCheck

    End Function

    'For any phone field, if the user has filled it out, verify that it's a valid phone number
    'currently, only the shop form has a phone field, but this function is in this
    'common file in case someone down the road decides they want to put phone numbers
    'on other forms
    Public Shared Function checkPhone(PhoneString As String) As Boolean
        Dim PhoneCheck As Boolean = True

        'Assumes all numbers will be US numbers
        'US numbers, without hyphens, are 10 digits. This test checks for that,
        'and if number is not 10 digits, returns an error result to the 
        'calling code
        If PhoneString <> Nothing And Not (PhoneString = "") Then
            Dim StrippedNumber As String = PhoneString.Replace("-", "")
            If Not (StrippedNumber.Length() = 10) Then
                PhoneCheck = False
            End If
        End If

        Return PhoneCheck

    End Function

    'Gets the URL of the current host serving up the forms
    'Used to determine whether to run development code or production code
    'in specific sections of all forms
    Public Shared Function getHost() As String
        Dim host As String = HttpContext.Current.Request.Url.Host

        '------------------------------------
        'use for testing live activity from test server - specifically, email changes
        'ONLY turn on if you want end users to receive email notifications from test forms on mercury
        'When not on, users only receive notification from froms in production

        'host = "apps.prairie.illinois.edu"
        '------------------------------------

        Return host
    End Function

    'Creates a WONumber to be used for a new request. Used by all forms.
    'Parameter expected is one of: "Shop", "Facilities", "Move"
    Public Shared Function getWONumber(ByVal FormType As String) As Integer

        Dim TableName As String = ""
        Dim TableSchema As String = ""
        Dim WONumberText As String = ""
        Dim WONumberInt As Integer

        Dim ConnString As String
        Dim QueryString As String
        Dim cmd As New SqlCommand

        'set connection info and schema info based on current host
        If getHost() = "apps.prairie.illinois.edu" Then
            ConnString = ConfigurationManager.ConnectionStrings("DatastormConnStr").ConnectionString
            TableSchema = "Forms"
        Else
            ConnString = ConfigurationManager.ConnectionStrings("HailConnStr").ConnectionString
            TableSchema = "dbo"
        End If

        'use form type to set table name for query
        Select Case FormType
            Case "Shop"
                TableName = TableSchema + ".ShopRequest"
            Case "Facilities"
                TableName = TableSchema + ".FacilitiesWorkRequest"
            Case "Move"
                TableName = TableSchema + ".MoveJob"

        End Select

        'Get the max work order number from relevant table for this request
        If FormType = "Shop" Or FormType = "Facilities" Then
            QueryString = "SELECT MAX(WONumber) FROM " + TableName
        Else
            'The highest WONumber from previous system for Move data is 5862. Because we can't move over the info
            'from the previous system due to drupal complications, the initial WONumber for
            'MoveJob is hard coded to an original number if Max number for MoveJob comes back null.
            'Not totally pleased with this work around, since there's an edge case where we may end up producing
            'duplicate work order numbers if we get a false null, but leaving for now due to
            'time constraint for delivery
            QueryString = "SELECT ISNULL(MAX(WONumber),'5863') FROM " + TableName
        End If

        Dim Conn As New SqlConnection(ConnString)
        Conn.Open()
        cmd = New SqlCommand(QueryString, Conn)
        WONumberText = cmd.ExecuteScalar()

        'Increment the result of the query by 1 to produce the next highest WONumber for the current request
        WONumberInt = CInt(WONumberText) + 1
        'WONumberText = CStr(WONumberInt)
        Return WONumberInt

    End Function

    'Based on which server we are running on, determine and return requested info
    'Parameter expected to be one of: "connection", "schema"
    Public Shared Function getConnectionInfo(info As String) As String
        Dim requestedInfo As String = ""

        If PRIForms.getHost() = "apps.prairie.illinois.edu" Then
            If info = "connection" Then
                requestedInfo = ConfigurationManager.ConnectionStrings("DatastormConnStr").ConnectionString
            ElseIf info = "schema" Then
                requestedInfo = "Forms"
            End If
        Else
            If info = "connection" Then
                requestedInfo = ConfigurationManager.ConnectionStrings("HailConnStr").ConnectionString
            ElseIf info = "schema" Then
                requestedInfo = "dbo"
            End If
        End If

        Return requestedInfo

    End Function

    'Based on current host, set emails for communication with F&S or dev
    'Emails only go to F&S if forms are on the production host
    'Parameters expected for requestType are: "facilities", "shop"
    'Parameters expected for addressType are: "to", "from"
    Public Shared Function getEmails(requestType As String, addressType As String) As List(Of String)
        Dim EmailList As New List(Of String)
        Dim Host As String = PRIForms.getHost()

        If Host = "apps.prairie.illinois.edu" Then
            If requestType = "facilities" And addressType = "to" Then
                'EmailList.Add("tipswor@illinois.edu")
                EmailList.Add("matt1966@illinois.edu")
                EmailList.Add("bryant2@illinois.edu")
                EmailList.Add("tgriest@illinois.edu")
                EmailList.Add("sfanta@illinois.edu")
            ElseIf requestType = "facilities" And addressType = "from" Then
                EmailList.Add("matt1966@illinois.edu")
            End If
            If requestType = "shop" And addressType = "to" Then
                'EmailList.Add("tipswor@illinois.edu")
                EmailList.Add("matt1966@illinois.edu")
                EmailList.Add("bryant2@illinois.edu")
                EmailList.Add("tgriest@illinois.edu")
                EmailList.Add("rblacker@illinois.edu")
                EmailList.Add("rpadilla@illinois.edu")
                EmailList.Add("bma10@illinois.edu")
                EmailList.Add("Prairie_PrePostAward@mx.uillinois.edu")
                EmailList.Add("Prairie-FiscalServices@mx.uillinois.edu")
            ElseIf requestType = "shop" And addressType = "from" Then
                EmailList.Add("matt1966@illinois.edu")
            End If

        Else
            EmailList.Add("afodom@illinois.edu")
        End If

        Return EmailList
    End Function

    'Get fund manager for the fund selected by user
    Public Shared Function getFundManager(cfop As String) As String

        Dim FundManager As String
        'Dim ConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString
        Dim ConnStr As String = ConfigurationManager.ConnectionStrings("DatastormFiscalConnStr").ConnectionString
        Dim Conn As New SqlConnection(ConnStr)

        Dim cmd As New SqlCommand
        Dim dataReader As SqlDataReader

        FundManager = ""

        Conn.Open()
        Dim query As String = "SELECT FundManager, FundManagerEmail FROM vwPRIDE_Purchreq_Fund_Info WHERE (cfop = '" & cfop & "');"

        'put requestor information in shared variables declared up top, so all subs can access it
        cmd = New SqlCommand(query, Conn)
        dataReader = cmd.ExecuteReader()
        While dataReader.Read()
            FundManager = dataReader.Item(0)
            FMEmail = dataReader.Item(1)
        End While

        Return FundManager

    End Function

    Public Shared Function getErrorMessage() As String
        Return errorMessageContent
    End Function
End Class




