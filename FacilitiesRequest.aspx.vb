Imports System.Data
Imports System.Data.SqlClient
Imports System.IO
Imports System.Net.Mail

Partial Class FacilitiesRequest
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
    Shared BuildingName As String
    Shared BuildingID As String
    Shared Room As String
    Shared WorkRequested As String
    Shared CompletionDt As Date
    Shared WONum As Integer
    Shared DateEntered As Date

    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        'set up default dates for calendar - on non-submission (first load),
        'set dates to current day. If form is being previewed, default
        'to what was entered by user -- DO WE STILL NEED TO DO THIS, SINCE WE NO LONGER PREVIEW??
        If Not (IsPostBack) Then
            CompletionDateExtender.SelectedDate = Date.Today
        Else
            CompletionDateExtender.SelectedDate = CompletionDate.Text
            Dim CtrlID As String = String.Empty
            If Request.Form("__EVENTTARGET") IsNot Nothing And
                Request.Form("__EVENTTARGET") <> String.Empty Then
                CtrlID = Request.Form("__EVENTTARGET")
                If CtrlID = "RequestedBy" Then
                    SetFocus(BuildingChoices)
                End If
            End If
            getData(sender, e)
        End If

        'populate building field from database
        If Not IsPostBack Then
            PRIForms.PopulateBuildingList(BuildingChoices)
        End If

        PRIForms.errorMessageContent = ""
    End Sub

    'When submit button is clicked, insert information from form into database, and send relevant emails
    Protected Sub Submit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles SubmitButton.Click

        'gets the text box that is being used for name auto-complete
        Dim RequestorTextBox As TextBox = TryCast(FindControl("RequestedBy"), TextBox)
        WONum = PRIForms.getWONumber("Facilities")
        Dim ServiceStatus As String = "Not Started"

        'see if user left any fields blank
        Dim BlankFields As New List(Of String)
        BlankFields = checkForBlanks()

        'see if user entered date in the past
        Dim DateCheck As Boolean
        DateCheck = PRIForms.checkDates(CompletionDt)

        DateEntered = DateTime.Now.Date

        'only do insert if fields are not blank and date is not in the past
        'otherwise issue error messages and do not proceed with insert
        If BlankFields.Count() = 0 And DateCheck = True Then
            'Insert form info into database
            Dim InsertStr As String

            Try
                Dim ConnString As String = PRIForms.getConnectionInfo("connection")
                Dim Schema As String = PRIForms.getConnectionInfo("schema")

                InsertStr = "INSERT INTO " & Schema & ".FacilitiesWorkRequest"
                InsertStr += "(RequestDate, RequestedBy, Building, BuildingID, RoomNumber, Description, RequestedCompletionDate, ServiceStatus, WONumber) "
                InsertStr += "VALUES (@RequestDate, @Requestor, @BuildingName, @BuildingID, @Room, @WorkRequested, @CompletionDt, @ServiceStatus, @WONumber)"

                Using conn As New SqlConnection(ConnString)
                    Using InsertCmd As New SqlCommand
                        InsertCmd.CommandType = CommandType.Text
                        InsertCmd.Connection = conn
                        InsertCmd.CommandText = InsertStr
                        InsertCmd.Parameters.AddWithValue("@RequestDate", DateEntered)
                        InsertCmd.Parameters.AddWithValue("@Requestor", Requestor)
                        InsertCmd.Parameters.AddWithValue("@BuildingName", BuildingName)
                        InsertCmd.Parameters.AddWithValue("@BuildingID", BuildingID)
                        InsertCmd.Parameters.AddWithValue("@Room", Room)
                        InsertCmd.Parameters.AddWithValue("@WorkRequested", WorkRequested)
                        InsertCmd.Parameters.AddWithValue("@CompletionDt", CompletionDt)
                        InsertCmd.Parameters.AddWithValue("@ServiceStatus", ServiceStatus)
                        InsertCmd.Parameters.AddWithValue("@WONumber", WONum)

                        conn.Open()
                        InsertCmd.ExecuteNonQuery()
                        conn.Close()
                    End Using
                End Using
            Catch ex As Exception
                PRIForms.errorMessageContent = "<b>Error submitting Work Request to database: </b></br></br>" & Server.HtmlEncode(ex.ToString())
                Response.Redirect("ErrorPage.aspx")
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
                PRIForms.errorMessageContent = "Error: Failed to retrieve submitter information. " & Server.HtmlEncode(ex.ToString())
                Response.Redirect("ErrorPage.aspx")
            End Try


            'put together email body for the email that will go to the people
            'who will process the request
            Dim EmailStr As String
            EmailStr = "<html><body style='font-size: 125%;'>"
            EmailStr += "<h2>Work Request Received</h2>"
            EmailStr += "<b>Work Order Number: </b>" & WONum & "<br/>"
            EmailStr += "<b>Requested By: </b>" & Requestor & " (" & RequestorEmail & ")<br />"
            EmailStr += "<b>Requested On: </b>" & DateEntered & "<br />"
            EmailStr += "<b>Building: </b>" & BuildingName & "<br />"
            EmailStr += "<b>Room: </b> " & Room & "<br />"
            EmailStr += "<b>Requested Completion Date: </b>" & CompletionDt & "<br/>"
            EmailStr += "<p><b>Work Requested:</b><br />"
            EmailStr += WorkRequested & "</p>"
            EmailStr += "</body></html>"

            'on success message, blank out all fields in the form
            PrintConfirmation(sender, e)

            'send relevant emails
            SendEmailConfirmation(EmailStr)
            SendProcessingEmail(EmailStr)
        ElseIf DateCheck = False Then 'if this happened, then requested date is in the past
            ResponseHeaderLabel.Text = "<div class='shadedErrorBox'><div class='feedbackHeader'>Uh oh. You chose a date in the past.</div> We have no time machines. Please try again.</div>"
            BlankFieldsLabel.Visible = False
        Else
            'cycles through list of field names returned by checkForBlanks and
            'prints them to the page for the user
            ResponseHeaderLabel.Text = "<div class='shadedErrorBox'><div class='feedbackHeader'>The following fields need to be filled out in order to submit request:</div>"
            Dim BlankFieldsString As String = "<ul>"
            For Each field In BlankFields
                BlankFieldsString += "<li>" & field & "</li>"
            Next
            BlankFieldsString += "</ul></div>"
            BlankFieldsLabel.Text = BlankFieldsString
        End If

    End Sub

    'prints confirmation of data for user 
    Protected Sub PrintConfirmation(ByVal sender As Object, ByVal e As EventArgs)
        BlankFieldsLabel.Text = ""
        ResponseHeaderLabel.Text = "<div class='shadedSuccessBox'><div class='feedbackHeader'>Your request has been submitted</div>"
        WONumberLabel.Text = "<b>Work Order Number: </b>" & WONum & "<br />"
        RequestedByLabel.Text = "<b>Requested By: </b>" & Requestor & "<br />"
        RequestedOnLabel.Text = "<b>Requested On: </b>" & DateEntered & "<br />"
        BuildingLabel.Text = "<b>Building: </b>" & BuildingName & "<br />"
        RoomLabel.Text = "<b>Room: </b> " & Room & "<br />"
        DescriptionLabel.Text = "<p><b>Work Requested:</b><br />" & WorkRequested & "</p>"
        CompletionDateLabel.Text = "<b>Requested Completion Date: </b>" & CompletionDt & "<br/></div>"
        RestartButton.Visible = True
        FullForm.Visible = False
    End Sub

    'restart to submit a new form
    Protected Sub Restart_Click(ByVal sender As Object, ByVal e As EventArgs) Handles RestartButton.Click
        Response.Redirect("FacilitiesRequest.aspx")
    End Sub

    'sends user back from PRI app server to intranet site on intranet server
    Protected Sub ReturnToIntranet_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ReturnToIntranet.Click
        Response.Redirect("https://staff-prairie.web.illinois.edu/")
    End Sub

    'In partnership with txt_RequestorName_TextChanged function, auto-completes requestor box
    <System.Web.Script.Services.ScriptMethod()>
    <System.Web.Services.WebMethod()>
    Public Shared Function GetNames(ByVal prefixText As String, ByVal count As Integer) As List(Of String)
        Return PRIForms.GetNames(prefixText, count)
    End Function

    'In partnership with getnames, autocompletes requestor box
    Public Sub RequestedByAutocomplete_TextChanged(sender As Object, e As EventArgs) 'Handles txt_RequestorName.TextChanged
        Try
            Dim txt_RequestorName_cb As TextBox
            txt_RequestorName_cb = TryCast(FindControl("RequestedBy"), TextBox)
            Dim staff_uin_lbl As Label = TryCast(FindControl("lbl_RequestorUIN"), Label)

            Dim txtbox_list As New List(Of TextBox)
            txtbox_list.Add(txt_RequestorName_cb)

            PRIForms.StaffAutocomplete_TextChanged(sender, e, txtbox_list, staff_uin_lbl)

        Catch ex As Exception
            PRIForms.errorMessageContent = "<b>Error acquiring Staff Requestor UIN:<b><br/><br/> " & ex.ToString()
            Response.Redirect("ErrorPage.aspx")
        End Try

    End Sub

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

                    'there should always be a requestor email, but if there is not, don't 
                    'add blank data to the message
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

                    mm.Subject = "Work Facilities Request Confirmation"

                    'To prevent uofi spam filters from flagging any emails sent
                    mm.Headers.Add("X-MessageSource", "Prairie Facilities Request")

                    mm.Body = body

                    mm.IsBodyHtml = True

                    Dim smtp As New SmtpClient()
                    smtp.Host = "express-smtp.cites.uiuc.edu"
                    smtp.Send(mm)
                End Using
            End Using
        Catch ex As Exception
            PRIForms.errorMessageContent += "<b>Error Sending PRI Facilities Work Request Summary Email to Requestor:</b><br/><br/>" + Server.HtmlEncode(ex.ToString())
            Response.Redirect("ErrorPage.aspx")
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

                    'address lists are filled based on form type and whether they are to/from emails
                    ToAddressList = PRIForms.getEmails("facilities", "to")
                    FromAddressList = PRIForms.getEmails("facilities", "from")

                    For Each address In ToAddressList
                        mm.To.Add(address)
                    Next

                    For Each address In FromAddressList
                        mm.From = New MailAddress(address)
                    Next

                    mm.Subject = "Action Required: Work Facilities Request"

                    'To prevent uofi spam filters from flagging any emails sent
                    mm.Headers.Add("X-MessageSource", "Prairie Facilities Request")

                    mm.Body = body

                    mm.IsBodyHtml = True

                    Dim smtp As New SmtpClient()
                    smtp.Host = "express-smtp.cites.uiuc.edu"
                    smtp.Send(mm)

                End Using
            End Using
        Catch ex As Exception
            PRIForms.errorMessageContent = "<b>Error Sending PRI Facilities Work Request Processing Email to Facilities:</b><br/><br/>" & Server.HtmlEncode(ex.ToString())
            Response.Redirect("ErrorPage.aspx")
        End Try

    End Sub

    'checks to see if user left any required fields blank
    'if any fields are blank, add them to the EmptyFields list
    'Returns list of field names for printing error to page
    Function checkForBlanks() As List(Of String)
        Dim EmptyFields As New List(Of String)

        If Requestor Is Nothing Or Requestor = "" Then
            EmptyFields.Add("Requested By")
        End If

        If BuildingName = "--Select Building--" Then
            EmptyFields.Add("Building")
        End If

        If Room Is Nothing Or Room = "" Then
            EmptyFields.Add("Room Number")
        End If

        If WorkRequested Is Nothing Or WorkRequested = "" Then
            EmptyFields.Add("Description of work to be completed")
        End If

        If CompletionDt = DateTime.MinValue Then
            EmptyFields.Add("Requested Completion Date")
        End If

        Return EmptyFields

    End Function

    'collects data submitted in form and assigns to global variables
    'to be used by all subs and functions
    Protected Sub getData(ByVal sender As Object, ByVal e As EventArgs)
        Dim RequestorTextBox As TextBox = TryCast(FindControl("RequestedBy"), TextBox)
        Requestor = RequestorTextBox.Text
        BuildingName = BuildingChoices.SelectedItem.Text
        BuildingID = BuildingChoices.SelectedItem.Value
        Room = RoomNumber.Text
        WorkRequested = Description.Text
        CompletionDt = CompletionDate.Text

    End Sub

End Class



