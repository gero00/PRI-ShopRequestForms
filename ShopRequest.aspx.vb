Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net.Mail
Imports System.Drawing
Imports System.Security
Imports System.Configuration


Partial Class ShopRequest
    Inherits System.Web.UI.Page

    'variables to hold lists of names for auto-complete element RequestedBy
    Shared staff_names As New List(Of String)()
    Shared staff_uins As New List(Of String)()

    'variables to hold information on person who submitted form
    Shared RequestorUsername As String = ""
    Shared RequestorEmail As String = ""
    Shared RequestorUIN As String = ""
    Shared RequestorFullName As String = ""
    Shared RequestorEmpId As Integer = 0

    'variables to hold record info when form is submitted
    Shared Requestor As String
    Shared CompletionDt As String
    Shared AcctInfo As String
    Shared AcctPI As String
    Shared WorkRequested As String
    Shared AprvdBy As String
    Shared WONum As Integer
    Shared DateEntered As Date
    Shared Justify As String


    'variables to hold information for account dropdown
    'Shared fopa As List(Of String) = New List(Of String) 'acct numbs
    Shared fopa As List(Of String) = New List(Of String)() 'acct numbs with activity description
    Shared fop_pi As List(Of String) = New List(Of String)() 'pi name

    'variables for PI emails that are used when sending email notifications
    Shared CFOP_proxy_email_str As String = "" 'Placeholder String listing all proxies for funds included in purchase request
    Shared CFOP_proxy_emails As New List(Of String)() 'CFOP_proxy_emails list contains the emails for all CFOP PI proxies for a particular fund. Used to populate ddl_CFOP_emails dropdown to notify for approval


    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        'if first load, set up default dates for calendar,
        'set dates to current day. If form is being previewed (, default
        'to what was entered by user - DO WE STILL NEED TO DO THIS SINCE THERE IS NO PREVIEW?
        If Not (IsPostBack) Then
            CompletionDateExtender.SelectedDate = Date.Today
            BlankFieldsLabel.Enabled = False
        Else
            Dim CtrlID As String = String.Empty
            If Request.Form("__EVENTTARGET") IsNot Nothing And
                Request.Form("__EVENTTARGET") <> String.Empty Then
                CtrlID = Request.Form("__EVENTTARGET")
                If CtrlID = "RequestedBy" Then
                    SetFocus(CompletionDate)
                End If
                If CtrlID = "txt_CFOP" Then
                    SetFocus(Description)
                End If
            End If

            CompletionDateExtender.SelectedDate = CompletionDate.Text
            getData(sender, e)
        End If

    End Sub

    'prints confirmation information for user
    Protected Sub PrintConfirmation(ByVal sender As Object, ByVal e As EventArgs)

        BlankFieldsLabel.Text = ""
        PreviewHeaderLabel.Text = "<div class='shadedSuccessBox'><h2>Your request has been submitted</h2></p>"
        WONumberLabel.Text = "<b>Work Order Number: </b>" & WONum & "<br />"
        RequestedByLabel.Text = "<b>Requested By: </b>" & Requestor & " (" & RequestorEmail & ") <br />"
        RequestedOnLabel.Text = "<b>Requested On: </b>" & DateEntered & "<br />"
        CompletionDateLabel.Text = "<b>Requested Completion Date: </b>" & CompletionDt & "<br/>"
        AccountInfoLabel.Text = "<b>Account Info: </b>" & AcctInfo & "<br/>"
        AccountPILabel.Text = "<b>Account PI: </b>" & AcctPI & " (" & CFOP_proxy_emails(0) & ")<br/>"
        FundMgrLabel.Text = "<b>Fund Manager: </b>" & PRIForms.FundManager & "( " & PRIForms.FMEmail & ") </p>"
        DescriptionLabel.Text = "<p><b>Work Requested:</b><br />" & WorkRequested & "</p>"
        JustifyLabel.Text = "<p><b>Business Justification:</b><br />" & Justify & "</p></div>"
        RestartButton.Visible = True
        FullForm.Visible = False

    End Sub

    'start a new form
    Protected Sub Restart_Click(ByVal sender As Object, ByVal e As EventArgs) Handles RestartButton.Click
        Response.Redirect("ShopRequest.aspx")
    End Sub

    'sends user back from PRI app server to intranet site on intranet serve
    Protected Sub ReturnToIntranet_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ReturnToIntranet.Click
        Response.Redirect("https://staff-prairie.web.illinois.edu/")
    End Sub

    'When submit button is clicked, insert information from form into database, and send relevant emails
    Protected Sub Submit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles SubmitButton.Click

        'gets the text box that is being used for name auto-complete
        Dim RequestorTextBox As TextBox = TryCast(FindControl("RequestedBy"), TextBox)

        'see if user left any fields blank
        Dim BlankFields As New List(Of String)
        BlankFields = checkForBlanks()

        'see if user entered date in the past
        Dim DateCheck As Boolean
        DateCheck = PRIForms.checkDates(CompletionDt)

        Dim PIEmail As String = ViewState("PIEmail")

        'only do insert if fields are not blank and date is not in the past
        'otherwise issue error messages and do not proceed with insert
        If BlankFields.Count() = 0 And DateCheck = True Then

            DateEntered = DateTime.Now.Date
            WONum = PRIForms.getWONumber("Shop")
            Dim InsertStr As String
            Dim ConnString As String = PRIForms.getConnectionInfo("connection")
            Dim Schema As String = PRIForms.getConnectionInfo("schema")

            Try
                InsertStr =
                        "INSERT INTO " & Schema & ".ShopRequest " &
                        "(RequestedBy, RequestedCompletionDate, Account, AccountPI, Description, PIEmail, ServiceStatus, WONumber, RequestDate, Justification, FundManager, FundManagerEmail) " &
                        "VALUES (@Requestor,  @CompletionDt, @AcctInfo, @AcctPI, @WorkRequested, @PIEmail, @ServiceStatus, @WONumber, @RequestDate, @Justification, @FundManager, @FundManagerEmail)"

                Using conn As New SqlConnection(ConnString)

                    Using InsertCmd As New SqlCommand
                        InsertCmd.CommandType = CommandType.Text
                        InsertCmd.Connection = conn
                        InsertCmd.CommandText = InsertStr
                        InsertCmd.Parameters.AddWithValue("@Requestor", Requestor)
                        InsertCmd.Parameters.AddWithValue("@CompletionDt", CompletionDt)
                        InsertCmd.Parameters.AddWithValue("@AcctInfo", AcctInfo)
                        InsertCmd.Parameters.AddWithValue("@AcctPI", AcctPI)
                        InsertCmd.Parameters.AddWithValue("@WorkRequested", WorkRequested)
                        InsertCmd.Parameters.AddWithValue("@PIEmail", PIEmail)
                        InsertCmd.Parameters.AddWithValue("@ServiceStatus", "Not Started")
                        InsertCmd.Parameters.AddWithValue("@WONumber", WONum)
                        InsertCmd.Parameters.AddWithValue("@RequestDate", DateEntered)
                        InsertCmd.Parameters.AddWithValue("@Justification", Justify)
                        InsertCmd.Parameters.AddWithValue("@FundManager", PRIForms.FundManager)
                        InsertCmd.Parameters.AddWithValue("@FundManagerEmail", PRIForms.FMEmail)

                        conn.Open()
                        'InsertCmd.Prepare()
                        InsertCmd.ExecuteNonQuery()
                        conn.Close()
                    End Using
                End Using
            Catch ex As Exception
                'MsgBox("Error: Shop Request submission failed. " & Server.HtmlEncode(ex.ToString()))
            End Try


            'Get information on user who requested the work
            Try
                'Dim RequestorConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString
				Dim RequestorConnStr As String = ConfigurationManager.ConnectionStrings("DatastormFiscalConnStr").ConnectionString
                Dim ReqConn As New SqlConnection(RequestorConnStr)

                Dim cmd As New SqlCommand
                Dim dataReader As SqlDataReader

                ReqConn.Open()
                Dim sqlGetRequestorInfo As String = "SELECT UINNoDash, EMail, EmpID, Name FROM vwStaffNamePurchRequestInfo WHERE (Name = '" & Requestor & "');"

                'put requestor information in shared variables declared up top, so all subs can access it
                cmd = New SqlCommand(sqlGetRequestorInfo, ReqConn)
                dataReader = cmd.ExecuteReader()
                While dataReader.Read()
                    RequestorUIN = dataReader.Item(0)
                    RequestorEmail = dataReader(1)
                    RequestorEmpId = dataReader(2)
                    RequestorFullName = dataReader.Item(3)
                End While
            Catch ex As Exception
                MsgBox("Error: Failed to retrieve submitter information. " & Server.HtmlEncode(ex.ToString()))
            End Try

            'put together email body for the email that will go to the people
            'who will process the request
            Dim EmailStr As String
            EmailStr = "<html><body style='font-size: 125%'>"
            EmailStr += "<h2>Shop Request Received</h2>"
            EmailStr += "<b>Work Order Number:</b> " & WONum & "<br />"
            EmailStr += "<b>Requested By: </b>" & Requestor & " (" & RequestorEmail & ")<br />"
            EmailStr += "<b>Requested On: </b>" & DateEntered & "<br />"
            EmailStr += "<b>Requested Completion Date: </b>" & CompletionDt & "<br/>"
            EmailStr += "<b>Account: </b>" & AcctInfo & "<br/>"
            EmailStr += "<b>Account PI: </b>" & AcctPI & " (" & CFOP_proxy_emails(0) & ")<br/>"
            EmailStr += "<b>Fund Manager: </b>" & PRIForms.FundManager & " (" & PRIForms.FMEmail & ") <br/>"
            EmailStr += "<p><b>Work Requested:</b><br />"
            EmailStr += WorkRequested & "</p>"
            EmailStr += "<p><b>Business Justification:</b><br />"
            EmailStr += Justify & "</p>"
            'closing tags for the email string are in the relevant subs

            'on success message, blank out all fields in the form
            PrintConfirmation(sender, e)

            'send relevant emails
            SendEmailConfirmation(EmailStr)
            SendProcessingEmail(EmailStr)
        Else
            'cycles through list of field names returned by checkForBlanks and
            'prints them to the page for the user
            PreviewHeaderLabel.Text = "<div class='shadedErrorBox'><h2>The following fields have errors that must be corrected before submitting request:</h2>"
            Dim BlankFieldsString As String = "<ul>"
            For Each field In BlankFields
                BlankFieldsString += "<li>" & field & " must be filled out</li>"
            Next
            If DateCheck = False Then
                BlankFieldsString += "<li> Requested Completion Date cannot be in the past</li>"
            End If
            BlankFieldsString += "</ul></div>"
            BlankFieldsLabel.Text = BlankFieldsString
        End If


    End Sub

    'In partnership with txt_RequestorName_TextChanged function, auto-completes boxes that require staff names
    <System.Web.Script.Services.ScriptMethod()>
    <System.Web.Services.WebMethod()>
    Public Shared Function GetNames(ByVal prefixText As String, ByVal count As Integer) As List(Of String)

        Try
            'Called function is in PRIForms.vb, globally accessible to all forms in this site.
            Return PRIForms.GetNames(prefixText, count)
        Catch ex As Exception
            MsgBox("Error: Unable to populate users drop down. " & ex.ToString())
        End Try


    End Function

    'Calls function in PRIForms that is shared across all forms in this site
    Public Sub StaffAutocomplete_TextChanged(sender As Object, e As EventArgs) 'Handles txt_RequestorName.TextChanged
        Try
            Dim txt_RequestorName_cb As TextBox
            Dim txt_AccountPI_cb As TextBox
            txt_RequestorName_cb = TryCast(FindControl("RequestedBy"), TextBox)
            txt_AccountPI_cb = TryCast(FindControl("AccountPI"), TextBox)

            Dim txtbox_list As New List(Of TextBox)
            txtbox_list.Add(txt_RequestorName_cb)
            txtbox_list.Add(txt_AccountPI_cb)

            Dim staff_uin_lbl As Label = TryCast(FindControl("lbl_RequestorUIN"), Label)

            'Called function is in PRIForms.vb, globally accessible to all forms in this site.
            PRIForms.StaffAutocomplete_TextChanged(sender, e, txtbox_list, staff_uin_lbl)
        Catch ex As Exception
            System.Diagnostics.Debug.Print("Error acquiring Staff Requestor UIN: " & ex.ToString())
        End Try

    End Sub

    'Get all data from this form
    Protected Sub getData(ByVal sender As Object, ByVal e As EventArgs)
        Dim RequestorTextBox As TextBox = TryCast(FindControl("RequestedBy"), TextBox)
        Requestor = RequestorTextBox.Text
        CompletionDt = CompletionDate.Text
        AcctInfo = txt_CFOP.Text
        AcctPI = AccountPI.Text
        WorkRequested = Description.Text
        Justify = Justification.Text
        PRIForms.FundManager = FundMgr.Text
    End Sub

    'if any fields are blank, add them to the EmptyFields list
    'Returns list of field names for printing to page
    Function checkForBlanks() As List(Of String)
        Dim EmptyFields As New List(Of String)

        If Requestor Is Nothing Or Requestor = "" Then
            EmptyFields.Add("Requested By")
        End If

        If CompletionDt Is Nothing Or CompletionDt = "" Then
            EmptyFields.Add("Requested Completion Date")
        End If

        If AcctInfo Is Nothing Or AcctInfo = "" Then
            EmptyFields.Add("Account Information")
        End If

        If WorkRequested Is Nothing Or WorkRequested = "" Then
            EmptyFields.Add("Description of work to be completed")
        End If

        Return EmptyFields

    End Function

    'sends email confirmation to person who submitted the request
    Protected Sub SendEmailConfirmation(body As String)
        Dim NamePieces As Array = Split(RequestorFullName, ",")
        Dim FirstName As String = NamePieces(1)
        Dim LastName As String = NamePieces(0)
        Try
            Using sw As New StringWriter
                Using hw As New HtmlTextWriter(sw)

                    Dim sr As New StringReader(sw.ToString())

                    Dim mm As New MailMessage()

                    'if requestor email is not blank - which should not happen - 
                    'don't add empty informaiton to email
                    If RequestorEmail <> Nothing And RequestorEmail <> "" Then
                        Dim ToAddress As String
                        Dim Host As String = PRIForms.getHost()
                        If Host = "apps.prairie.illinois.edu" Then
                            ToAddress = RequestorEmail
                        Else
                            ToAddress = "afodom@illinois.edu"
                        End If
                        mm.To.Add(ToAddress)
                    End If

                    Dim FromAddressList As New List(Of String)
                    FromAddressList = PRIForms.getEmails("facilities", "from")

                    For Each address In FromAddressList
                        mm.From = New MailAddress(address)
                    Next

                    mm.Subject = "Shop Request Received"

                    'To prevent uofi spam filters from flagging any emails sent
                    mm.Headers.Add("X-MessageSource", "Prairie Purchase Request")

                    mm.Body = body
                    mm.Body += "<p style='font-style:italic'>Your PI has been sent a copy of your request for review."
                    mm.Body += "Unless you hear otherwise, your request will be taken care of. If any of the information in this request is inaccurate, contact"
                    mm.Body += " Matt Thompson (matt1966@illinois.edu)</p>"
                    mm.Body += "</body></html>"

                    mm.IsBodyHtml = True

                    Try
                        Dim smtp As New SmtpClient()
                        smtp.Host = "express-smtp.cites.uiuc.edu"
                        smtp.Send(mm)
                    Catch ex As Exception
                        MsgBox("Error: Unable to send email confirmation to requestor. " & Server.HtmlEncode(ex.ToString()))
                    End Try

                End Using
            End Using
        Catch ex As Exception
            MsgBox("Error Sending PRI Shop Request Request Confirmation Email - " & Server.HtmlEncode(ex.ToString()))
        End Try

    End Sub

    'sends email notification of request to whomever will process said request
    Protected Sub SendProcessingEmail(body As String)
        Try
            Using sw As New StringWriter
                Using hw As New HtmlTextWriter(sw)

                    Dim sr As New StringReader(sw.ToString())

                    Dim mm As New MailMessage()

                    Dim ToAddressList As New List(Of String)
                    Dim FromAddressList As New List(Of String)
                    ToAddressList = PRIForms.getEmails("shop", "to")

                    If RequestorEmail <> Nothing And RequestorEmail <> "" Then
                        mm.From = New MailAddress(RequestorEmail)
                    End If

                    For Each address In ToAddressList
                        mm.To.Add(address)
                    Next

                    If PRIForms.getHost() = "apps.prairie.illinois.edu" Then
                        mm.To.Add(CFOP_proxy_emails(0))
                    End If


                    mm.Subject = "Action Required: Shop Facilities Request"

                    'To prevent uofi spam filters from flagging any emails sent
                    mm.Headers.Add("X-MessageSource", "Prairie Purchase Request")


                    mm.Body = body
                    mm.Body += "<p><i><b>Account PI/Fund Manager</b> - "
                    mm.Body += "if you have any concerns with this request, please 'Reply All' to share those concerns.</i></p>"
                    mm.Body += "</body></html>"

                    mm.IsBodyHtml = True

                    'Need error handling code for email send failure?
                    Try
                        Dim smtp As New SmtpClient()
                        smtp.Host = "express-smtp.cites.uiuc.edu"
                        smtp.Send(mm)
                    Catch ex As Exception
                        MsgBox("Error: Unable to email shop request to PRI. " & Server.HtmlEncode(ex.ToString()))
                    End Try

                End Using
            End Using
        Catch ex As Exception
            MsgBox("Error Sending PRI Shop Request Processing Email - " & Server.HtmlEncode(ex.ToString()))
        End Try

    End Sub


    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'event handler for CFOP field changing in Funding Information grid. Populate PI textbox and notification email dropdown based on fund
    'code taken from purchase form, written by Greg Rogers
    'not totally sure what specifically is happening in all aspects of this code, but it works.
    Public Sub txt_CFOP_TextChanged(sender As Object, e As EventArgs)
        Dim cfop_index As Integer
        Try

            Dim i As Integer = 0 '= sender.EditIndex
            Dim cfop_txt As String = txt_CFOP.Text.Trim

            'if a string has been entered in the cfop textbox populate autoextender dropdown
            If cfop_txt <> Nothing And cfop_txt <> "" Then
                SearchCFOPS(cfop_txt, 1)
                cfop_index = fopa.IndexOf(cfop_txt)
                If cfop_txt.Count > 25 Then
                    cfop_txt = cfop_txt.Substring(0, 26)
                    txt_CFOP.Text = cfop_txt
                End If

                'if the cfop_index is valid, and the pi list has something in it
                'get PI name and add to AccountPI textbox
                If cfop_index <> Nothing And cfop_index <> -1 Then
                    If fop_pi.Count <> 0 And fop_pi.Count > cfop_index Then
                        If fop_pi(cfop_index) <> Nothing Then
                            AccountPI.Text = fop_pi(cfop_index)
                            FundMgr.Text = PRIForms.getFundManager(cfop_txt)
                        Else
                            AccountPI.Text = ""
                        End If
                    Else
                        AccountPI.Text = ""
                    End If
                Else

                    If cfop_txt.Count > 25 Then
                        SearchCFOPS(cfop_txt, 1)
                        If fop_pi.Count <> 0 And fop_pi.Count > cfop_index And cfop_index >= 0 Then
                            If fop_pi(cfop_index) <> Nothing Then
                                AccountPI.Text = fop_pi(cfop_index)
                                FundMgr.Text = PRIForms.getFundManager(cfop_txt)
                            Else
                                AccountPI.Text = ""
                            End If
                        Else
                            AccountPI.Text = ""
                        End If
                        If AccountPI.Text = "" Then
                            SearchCFOPS(cfop_txt, 1)
                            If fop_pi.Count <> 0 Then
                                AccountPI.Text = fop_pi(0)
                                FundMgr.Text = PRIForms.getFundManager(cfop_txt)
                            Else
                                AccountPI.Text = "No PI found. Please enter valid CFOPA."
                                AccountPI.ForeColor = System.Drawing.Color.Red
                            End If
                        End If
                        GetPIEmail(cfop_txt)
                    End If
                End If
                'GetPIEmail(cfop_txt)

            End If


        Catch ex As Exception

            MsgBox("Error acquiring CFOP PI: " & ex.ToString() & " fop_pi count: " & fop_pi.Count & " index: " & cfop_index.ToString())
            System.Diagnostics.Debug.Print("Error acquiring CFOP PI: " & ex.ToString())
        End Try

    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' The asp:AutoCompleteExtender AJAX control integrated into the txtCFOP textbox in the GridviewFund gridview consumes the WebService SearchCFOPS. 
    ' There is also an event handler, txt_CFOP_TextChanged, for the txt_CFOP control's OnTextChanged event that also
    ' calls Search_CFOPS in case the user does not select a menu item from the generated dropdownlist and types or pastes the CFOP into the textbox directly.

    'load the CFOP field in the Funding Information grid using the AutoCompleteExtender control
    <System.Web.Script.Services.ScriptMethod(), System.Web.Services.WebMethod()>
    Public Shared Function SearchCFOPS(ByVal prefixText As String, ByVal count As Integer) As List(Of String)

        fopa.Clear()
        'fopa_activity.Clear()
        fop_pi.Clear()

        Dim CFOP As String
        Dim Activity As String

        'Dim ConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString
		Dim ConnStr As String = ConfigurationManager.ConnectionStrings("DatastormFiscalConnStr").ConnectionString
        Dim Conn As New SqlConnection(ConnStr)

        Dim cmd As New SqlCommand
        Dim dataReader As SqlDataReader

        Conn.Open()

        'Dim CFOP_to_match As String
        'CFOP_to_match = prefixText.Substring(0, 26)

        Dim query As String = "SELECT CFOP, ActivityDesc, PI FROM vwPRIDE_Purchreq_Fund_Info WHERE (cfop LIKE '" & prefixText.Trim & "%');"

        'put requestor information in shared variables declared up top, so all subs can access it
        cmd = New SqlCommand(query, Conn)
        dataReader = cmd.ExecuteReader()

        While dataReader.Read

            CFOP = Replace(dataReader("CFOP").ToString, Chr(13), "")
            Activity = Replace(dataReader("ActivityDesc").ToString, Chr(13), "")
            Activity = Replace(Activity, Chr(10), "")
            fopa.Add(CFOP & " --" & Activity)
            fop_pi.Add(dataReader("PI").ToString)

        End While
        Conn.Close()

        Return fopa
    End Function

    'Get emails for all proxies for all funds included in purchase and populate CFOP_pi_proxy_emails list and CFOP_proxy_email_str string variables and populate the dropdownlist of proxy contacts in the GridViewFund gridview
    Protected Sub GetPIEmail(cfop_str As String)

        CFOP_proxy_email_str = ""
        CFOP_proxy_emails.Clear()
        Dim CFOP_str_all As String = ""
        'Dim ConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString
		Dim ConnStr As String = ConfigurationManager.ConnectionStrings("DatastormFiscalConnStr").ConnectionString

        Dim objConnection As New SqlConnection(ConnStr)
        Dim cmd As New SqlCommand
        objConnection.Open()

        Try

            Dim current_fiscal_year As Integer = 0
            Dim sql_current_fiscal_year As String = "SELECT FiscalYearforObs AS CurrentFiscalYear FROM Divisions WHERE (ID = 6);"

            cmd = New SqlCommand(sql_current_fiscal_year, objConnection)
            current_fiscal_year = cmd.ExecuteScalar()
            cmd.Dispose()


            Dim sql_get_proxy_emails As String = "SELECT DISTINCT S.Email FROM vwStaffNamePurchRequestInfo S"
            sql_get_proxy_emails += " JOIN vwUniversityFundsProxy U On U.ProxyEmpID=S.EmpID WHERE (U.CFOP = '" & cfop_str & "')"
            sql_get_proxy_emails += " And (fiscalyear = " & current_fiscal_year.ToString() & " Or fiscalyear = 0) AND ProxyType='PI';"

            cmd = New SqlCommand(sql_get_proxy_emails, objConnection)
            CFOP_proxy_emails.Add(cmd.ExecuteScalar)

            cmd.Dispose()
            objConnection.Close()

            ViewState("PIEmail") = CFOP_proxy_emails(0)
        Catch ex As Exception
            MsgBox("Error determining proxies to populate dropdownlist: " & Server.HtmlEncode(ex.ToString()))
            System.Diagnostics.Debug.Print("Error determining proxies to populate dropdownlist: " & ex.ToString())
        End Try
    End Sub


End Class
