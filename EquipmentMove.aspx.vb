Imports System.Data
Imports System.Data.SqlClient
Imports System.Configuration
Imports System.Collections.Generic
Imports System.IO
Imports System.Net.Mail
Imports System.Security
Imports System.Web
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports System.Drawing
Partial Class EquipmentMove
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

    Shared ptag_list As New List(Of String)
    Shared ptag_descriptions As New List(Of String)

    'variables to hold record info when form is submitted
    Shared SurveyName As String
    Shared Requestor As String
    Shared PhoneNumber As String
    Shared CompletionDt As String
    Shared dt As DataTable
    Shared ddlType As New DropDownList
    Shared ddlTypelist As New List(Of String)({"--Select Type--", "Move/Transfer", "To Trash/Surplus"})
    Shared ddlBuildings As New DropDownList
    Shared Custodian As String
    Shared SerialNumber As String
    Shared NotesText As String
    Shared FoundRowBlanks As Boolean = False
    Shared FocusRowIndex As Integer

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    'To override the default windows msgbox which will not work with IIS webapps
    Private Sub MesgBox(ByVal sMessage As String)

        Dim msg As String
        msg = "<script language='javascript'>"
        msg += "alert('" & sMessage & "');"
        msg += "<" & "/script>"
        Response.Write(msg)

        Dim msg2 As String                          ' http://jsbin.com/ibUrIxu/1/edit
        msg2 = "function customAlert(msg,duration)"
        msg2 += "{"
        msg2 += "    var styler = document.createElement('div');"
        msg2 += "    styler.setAttribute('style','border: solid 5px Red;width:auto;height:auto;top:50%;left:40%;background-color:#444;color:Silver');"
        msg2 += "    styler.innerHTML = '<h1>'+msg+'</h1>';"
        msg2 += "    setTimeout(function()"
        msg2 += "    {"
        msg2 += "        styler.parentNode.removeChild(styler);"
        msg2 += "    },duration);"
        msg2 += "    document.body.appendChild(styler);"
        msg2 += "}"
        msg2 += "function caller()"
        msg2 += "{"
        msg2 += "customAlert('This custom alert box will be closed in 2 seconds','2000');"
        msg2 += "}"
        msg2 += "</script>"
        'Response.Write(msg2)

    End Sub

    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        If Not Me.IsPostBack Then

            ptag_list.Clear()
            ptag_descriptions.Clear()
            PTagMissing.Text = ""
            LoadTable(sender, e)

            CompletionDateExtender.SelectedDate = Date.Today

            PRIForms.PopulateBuildingList(ddlFromBuilding)
            PRIForms.PopulateBuildingList(ddlToBuilding)
            ddlFromBuilding.Items.Insert(1, New ListItem("Surplus", "1"))
            ddlToBuilding.Items.Insert(1, New ListItem("To Trash/Surplus", "1"))

            'populate the move type dropdown list with the data in ddl_list
            ddlMoveType.DataSource = ddlTypelist
            ddlMoveType.DataBind()

        Else
            CompletionDateExtender.SelectedDate = CompletionDate.Text
            dt = ViewState("CurrentTable")
            RequestGrid.DataSource = dt
            RequestGrid.DataBind()
            getData(sender, e)
        End If

    End Sub

    Protected Sub Restart_Click(ByVal sender As Object, ByVal e As EventArgs) Handles RestartButton.Click
        Response.Redirect("EquipmentMove.aspx")

    End Sub

    Protected Sub ReturnToIntranet_Click(ByVal sender As Object, ByVal e As EventArgs) Handles ReturnToIntranet.Click
        Response.Redirect("https://staff-prairie.web.illinois.edu/")

    End Sub

    'In partnership with txt_RequestorName_TextChanged function, auto-completes requestor box
    <System.Web.Script.Services.ScriptMethod()>
    <System.Web.Services.WebMethod()>
    Public Shared Function GetNames(ByVal prefixText As String, ByVal count As Integer) As List(Of String)

        'The connection info for for connecting to the database where the staff names are
        Try
            'MsgBox("GetNames entered")
            Return PRIForms.GetNames(prefixText, count)

        Catch ex As Exception
            MsgBox("Error: Unable to populate users drop down. " & ex.ToString())
        End Try


    End Function

    'In partnership with getnames, autocompletes requestor box
    Public Sub StaffAutocomplete_TextChanged(sender As Object, e As EventArgs) 'Handles txt_RequestorName.TextChanged
        Try
            dt.Clear()
            Dim txt_RequestorName_cb As TextBox
            txt_RequestorName_cb = TryCast(FindControl("RequestedBy"), TextBox)
            Dim staff_uin_lbl As Label = TryCast(FindControl("lbl_RequestorUIN"), Label)

            Dim txtbox_list As New List(Of TextBox)
            txtbox_list.Add(txt_RequestorName_cb)

            'Called function is in PRIForms, globally accessible to all forms in this site
            PRIForms.StaffAutocomplete_TextChanged(sender, e, txtbox_list, staff_uin_lbl)

        Catch ex As Exception
            System.Diagnostics.Debug.Print("Error acquiring Staff Requestor UIN: " & ex.ToString())
        End Try

    End Sub

    Public Sub CustodianAutocomplete_TextChanged(sender As Object, e As EventArgs) 'Handles txt_RequestorName.TextChanged
        Try
            Dim CustodianControl As TextBox = TryCast(FindControl("Custodian"), TextBox)
            Dim staff_uin_lbl As Label = TryCast(FindControl("lbl_RequestorUIN"), Label)

            Dim txtbox_list As New List(Of TextBox)
            txtbox_list.Add(CustodianControl)

            'Called function is in PRIForms, globally accessible to all forms in this site
            PRIForms.StaffAutocomplete_TextChanged(sender, e, txtbox_list, staff_uin_lbl)

        Catch ex As Exception
            System.Diagnostics.Debug.Print("Error acquiring Staff Requestor UIN: " & ex.ToString())
        End Try

    End Sub

    Protected Sub AddItem_Button(ByVal sender As Object, ByVal e As EventArgs) Handles AddItemButton.Click

        ErrorHeaderItems.Text = ""
        AddItemErrorLabel.Text = ""

        'see if user left any fields blank
        Dim BlankFields As New List(Of String)
        BlankFields = CheckForBlanks("item")

        If BlankFields.Count > 0 Then
            Dim BlankFieldsString As String

            ErrorHeaderItems.Text = "<div class='shadedErrorBox' style='width:1330px;'><h2>"
            ErrorHeaderItems.Text += "Row cannot be added</h2>"

            BlankFieldsString = "<p><b>The following information is required for each item being moved</b></p>"
            BlankFieldsString += "<ul>"
            For Each field In BlankFields
                BlankFieldsString += "<li>" & field & "</li>"
            Next
            BlankFieldsString += "</ul></div>"
            AddItemErrorLabel.Text += BlankFieldsString
        ElseIf ViewState("CurrentTable") IsNot Nothing Then

            'dt = New DataTable
            dt = TryCast(ViewState("CurrentTable"), DataTable)
            Dim dr As DataRow

            dr = dt.NewRow
            'store the values in the datatable
            If ddlMoveType.SelectedValue <> "To Trash/Surplus" Then
                dr("MoveType") = ddlMoveType.SelectedValue
            Else
                dr("MoveType") = "Surplus"
            End If

            dr("FromBuilding") = PRIForms.GetCleanBuildingString(ddlFromBuilding.SelectedItem.Text)
            dr("FromBuildingID") = ddlFromBuilding.SelectedItem.Value
            dr("FromRoom") = tbFromRoom.Text
            dr("ToBuilding") = PRIForms.GetCleanBuildingString(ddlToBuilding.SelectedItem.Text)
            dr("ToBuildingID") = ddlFromBuilding.SelectedItem.Value
            dr("ToRoom") = tbRoomNumber.Text
            dr("PTag") = acPTag.Text
            dr("SerialNumber") = tbSerialNumber.Text
            dr("Description") = tbDescription.Text
            dr("Custodian") = acCustodian.Text

            dt.Rows.Add(dr)
            ViewState("CurrentTable") = dt
            RequestGrid.DataSource = dt
            RequestGrid.DataBind()

            ddlMoveType.SelectedIndex = 0
            ddlFromBuilding.SelectedIndex = 0
            ddlToBuilding.SelectedIndex = 0
            ddlFromBuilding.SelectedIndex = 0
            tbFromRoom.Text = ""
            tbRoomNumber.Text = ""
            acCustodian.Text = ""
            acPTag.Text = ""
            tbSerialNumber.Text = ""
            tbDescription.Text = ""

            tbRoomNumber.BackColor = Color.White
            tbRoomNumber.Enabled = True
            tbFromRoom.BackColor = Color.White
            tbFromRoom.Enabled = True
            ddlFromBuilding.BackColor = Color.White
            ddlFromBuilding.Enabled = True
            ddlToBuilding.BackColor = Color.White
            ddlToBuilding.Enabled = True
            acCustodian.BackColor = Color.White
            acCustodian.Enabled = True
            tbDescription.BackColor = Color.White
            tbDescription.Enabled = True

            MistakeInstructions.Visible = True
        End If


    End Sub

    'In partnership with PTag_TextChanged function, auto-completes requestor box
    <System.Web.Script.Services.ScriptMethod()>
    <System.Web.Services.WebMethod()>
    Public Shared Function GetPTag(ByVal prefixText As String, ByVal count As Integer) As List(Of String)

        ptag_list.Clear()
        ptag_descriptions.Clear()

        'The connection info for for connecting to the database where the Ptag names are
        Try

            Dim RequestorConnStr As String = ConfigurationManager.ConnectionStrings("DatastormConnStr").ConnectionString
            Dim ReqConn As New SqlConnection(RequestorConnStr)
            Dim cmd As New SqlCommand
            Dim dataReader As SqlDataReader
            Dim ptag As String = ""


            ReqConn.Open()

            'Description column name is spelled wrong in the table, so it is also spelled wrong here.
            Dim sql_get_ptag_description As String = "Select PTagPerm, Desciption from Forms.PRIFixedAssets_EDW_Equipment where" & " PTagPerm Like '" & prefixText & "%';"

            cmd = New SqlCommand(sql_get_ptag_description, ReqConn)
            dataReader = cmd.ExecuteReader()

            'add all items returned from the database to the autocomplete element
            While dataReader.Read()
                ptag = dataReader.Item(0)
                ptag_list.Add(dataReader.Item(0))
                ptag_descriptions.Add(dataReader.Item(1))
            End While
            dataReader.Close()
            cmd.Dispose()

            Return ptag_list

        Catch ex As Exception
            MsgBox("Error: Unable to populate ptag drop down. " & ex.ToString())
            Return Nothing
        End Try
    End Function

    Protected Sub PTag_TextChanged(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Dim PTagText As String = acPTag.Text

            Dim PTagIndex As Integer

            If PTagText <> Nothing And PTagText <> "" Then
                PTagIndex = ptag_list.IndexOf(PTagText)
                PTagMissing.Text = ""
                PTagMissing.CssClass = ""
                Response.Write("ptag index set")
                If PTagIndex >= 0 Then
                    If ptag_list(PTagIndex) <> Nothing And ptag_list(PTagIndex) <> "" Then
                        acPTag.Text = ptag_list(PTagIndex)
                        tbDescription.Text = ptag_descriptions(PTagIndex)
                        tbDescription.Enabled = False
                        tbDescription.BackColor = Color.LightGray
                        tbDescription.ForeColor = Color.Black
                    End If
                Else
                    PTagMissing.Text = "PTag not found. If you are sure you have entered it correctly, provide a description of the item so we can update our records"
                    PTagMissing.CssClass = "shadedErrorBox"
                End If
            Else

            End If


        Catch ex As Exception
            System.Diagnostics.Debug.Print("Error filling description: " & ex.ToString())
            Response.Write("Error Filling Description")
        End Try

    End Sub

    Protected Sub MoveType_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        ErrorHeaderItems.Text = ""
        AddItemErrorLabel.Text = ""

        Try

            If ddlMoveType.SelectedValue = "To Trash/Surplus" Then
                ddlToBuilding.SelectedValue = "1"
                ddlToBuilding.BackColor = Color.LightGray
                ddlToBuilding.Enabled = False
                tbRoomNumber.Text = ""
                tbRoomNumber.BackColor = Color.LightGray
                tbRoomNumber.Enabled = False
                acCustodian.Text = ""
                acCustodian.BackColor = Color.LightGray
                acCustodian.Enabled = False
            Else
                ddlToBuilding.BackColor = Color.White
                ddlToBuilding.SelectedValue = "0"
                ddlToBuilding.Enabled = True
                tbRoomNumber.Text = ""
                tbRoomNumber.BackColor = Color.White
                tbRoomNumber.Enabled = True
                acCustodian.Text = ""
                acCustodian.BackColor = Color.White
                acCustodian.Enabled = True
            End If

        Catch ex As Exception
            System.Diagnostics.Debug.Print("Error filling description: " & ex.ToString())
            Response.Write("Error Selecting To Building")
        End Try

    End Sub

    Protected Sub FromBuilding_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        If ddlFromBuilding.SelectedValue = "1" Then
            tbFromRoom.Text = ""
            tbFromRoom.BackColor = Color.LightGray
            tbFromRoom.Enabled = False
        Else
            tbFromRoom.Text = ""
            tbFromRoom.BackColor = Color.White
            tbFromRoom.Enabled = True

        End If
    End Sub


    Protected Sub Submit_Button(ByVal sender As Object, ByVal e As EventArgs) Handles Submit.Click
        PTagMissing.Text = ""
        PTagMissing.CssClass = ""
        ErrorHeaderLabel.Text = ""
        SubmissionErrorLabel.Text = ""
        SurveyName = SurveyChoice.Text
        Requestor = RequestedBy.Text
        PhoneNumber = Phone.Text
        CompletionDt = CompletionDate.Text
        NotesText = Notes.Text
        dt = TryCast(ViewState("CurrentTable"), DataTable)
        Dim JobID As Integer = 0

        'see if user left any fields blank
        Dim BlankFields As New List(Of String)
        BlankFields = CheckForBlanks("job")

        'see if user entered date in the past
        Dim DateCheck As Boolean
        DateCheck = PRIForms.checkDates(CompletionDt)

        Dim PhoneCheck As Boolean
        PhoneCheck = PRIForms.checkPhone(PhoneNumber)

        Dim FoundErrors As Boolean = False


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

        'only do insert if fields are not blank and date is not in the past
        'otherwise issue error messages and do not proceed with insert
        If BlankFields.Count() = 0 And DateCheck = True Then
            'Insert base data for the job into MoveJob table - this is the data that only appears once
            'per move job - Survey, Requestor, Phone, From Bldg, From Room, Requested Completion Date
            Try
                Dim ConnString As String = PRIForms.getConnectionInfo("connection")
                Dim Schema As String = PRIForms.getConnectionInfo("schema")
                Dim CmdString As String
                Dim WONum As Integer = PRIForms.getWONumber("Move")
                Dim DateEntered As Date = DateTime.Now.Date

                CmdString = "INSERT INTO " & Schema & ".MoveJob"
                CmdString += "(Survey, RequestedBy, Phone, RequestedCompletionDate, Notes, ServiceStatus, WONumber, RequestDate) "
                CmdString += "OUTPUT inserted.id "
                CmdString += "VALUES (@Survey, @Requestor, @Phone, @CompletionDt, @Notes, @ServiceStatus, @WONumber, @RequestDate)"

                Dim conn As New SqlConnection(ConnString)

                Using cmd As New SqlCommand
                    cmd.CommandType = CommandType.Text
                    cmd.Connection = conn
                    cmd.CommandText = CmdString
                    cmd.Parameters.AddWithValue("@Survey", SurveyName)
                    cmd.Parameters.AddWithValue("@Requestor", Requestor)
                    cmd.Parameters.AddWithValue("@Phone", PhoneNumber)
                    cmd.Parameters.AddWithValue("@CompletionDt", CompletionDt)
                    cmd.Parameters.AddWithValue("@Notes", NotesText)
                    cmd.Parameters.AddWithValue("@ServiceStatus", "Not Started")
                    cmd.Parameters.AddWithValue("@WONumber", WONum)
                    cmd.Parameters.AddWithValue("@RequestDate", DateEntered)
                    conn.Open()

                    Dim reader As SqlDataReader = cmd.ExecuteReader
                    While reader.Read()
                        JobID = reader.Item(0)
                    End While

                    conn.Close()
                End Using

                'Insert all rows from the RequestGrid into the database with the JobID collected
                'in job insert statement above
                If ddlMoveType.SelectedValue <> "--Select Type--" Then
                    AddItem_Button(sender, e)
                End If

                CmdString = "INSERT INTO " & Schema & ".MoveItems"
                CmdString += "(JobID, MoveType, FromBuilding, FromBuildingID, FromRoom, ToBuilding, "
                CmdString += "BuildingID, Custodian, ToRoom, PTag, SerialNumber, Description) "
                CmdString += "VALUES (@JobID, @MoveType, @FromBuilding, @FromBuildingID, @FromRoom, @ToBuilding, "
                CmdString += "@BuildingID, @Custodian, @ToRoom, @PTag, @SerialNumber, @Description)"

                For Each row As DataRow In dt.Rows

                    Using cmd As New SqlCommand
                        cmd.CommandType = CommandType.Text
                        cmd.Connection = conn
                        cmd.CommandText = CmdString
                        cmd.Parameters.AddWithValue("@JobID", JobID)
                        cmd.Parameters.AddWithValue("@MoveType", row.Item("MoveType"))
                        cmd.Parameters.AddWithValue("@FromBuilding", row.Item("FromBuilding"))
                        cmd.Parameters.AddWithValue("@FromBuildingID", row.Item("FromBuildingID"))
                        cmd.Parameters.AddWithValue("@FromRoom", row.Item("FromRoom"))
                        cmd.Parameters.AddWithValue("@ToBuilding", row.Item("ToBuilding"))
                        cmd.Parameters.AddWithValue("@BuildingID", row.Item("ToBuildingID"))
                        cmd.Parameters.AddWithValue("@Custodian", row.Item("Custodian"))
                        cmd.Parameters.AddWithValue("@ToRoom", row.Item("ToRoom"))
                        cmd.Parameters.AddWithValue("@PTag", row.Item("PTag"))
                        cmd.Parameters.AddWithValue("@SerialNumber", row.Item("SerialNumber"))
                        cmd.Parameters.AddWithValue("@Description", row.Item("Description"))

                        conn.Open()

                        cmd.ExecuteNonQuery()

                        conn.Close()

                    End Using
                Next

            Catch ex As Exception
                MsgBox("Error: Work Request submission failed. " & Server.HtmlEncode(ex.ToString()))
            End Try
        Else
            FoundErrors = True
            ErrorHeaderLabel.Text = "<div class='shadedErrorBox'><h2>"
            ErrorHeaderLabel.Text += "There are errors in your form, and your request was not submitted. Please correct And resubmit.</h2>"
        End If

        If BlankFields.Count > 0 Then
            Dim BlankFieldsString As String

            BlankFieldsString = "<p><b>The following primary information Is required</b></p>"
            BlankFieldsString += "<ul>"
            For Each field In BlankFields
                BlankFieldsString += "<li>" & field & "</li>"
            Next
            BlankFieldsString += "</ul>"
            SubmissionErrorLabel.Text += BlankFieldsString
        End If

        If PhoneCheck = False Then
            SubmissionErrorLabel.Text += "<p><b>Your phone number must be 10 digits, including area code, And formatted ###-###-####</b></p>"
        End If

        If DateCheck = False Then
            SubmissionErrorLabel.Text += "<p><b>You chose a requested completion date in the past. Please choose a valid date.</b></p>"
        End If

        If FoundErrors Then
            SubmissionErrorLabel.Text += "</div>"
        Else
            ProcessSubmission(sender, e, JobID)
        End If

    End Sub

    Protected Sub ProcessSubmission(ByVal sender As Object, ByVal e As EventArgs, JobID As Integer)
        Dim TableString As String

        'variables for building email that gets sent to F&S
        Dim EmailString As String = ""
        Dim MoveTableStartString As String = ""
        Dim CloseTableString As String = "</table>"
        Dim SurplusTableStartString As String = ""
        Dim ItemString = ""
        Dim PreviousBuilding As String = ""
        Dim CurrentBuilding As String = ""
        Dim SectionTitleString As String = ""
        Dim td_formatting_100 As String = "<td align='left' width='100px' style='border-bottom:1px solid black;padding-left:10px; padding-right:10px;'>"
        Dim td_formatting_300 As String = "<td align='left' width='300px' style='border-bottom:1px solid black;padding-left:10px; padding-right:10px;'>"

        Dim Survey As String = ""
        Dim RequestedBy As String = ""
        Dim Phone As String = ""
        Dim FromBuilding As String = ""
        Dim FromBuildingID As String = ""
        Dim FromRoom As String = ""
        Dim CompletionDt As String = ""
        Dim NotesText As String = ""
        Dim RequestDate As String = ""
        Dim WONum As Integer = 0

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")
        Dim CmdString As String

        'Use output phrase to capture id generated by insert to use for insert into the connected MoveItems table.
        CmdString = "SELECT id, Survey, RequestedBy, Phone, RequestedCompletionDate, Notes, WONumber, RequestDate "
        CmdString += "FROM " & Schema & ".MoveJob WHERE id = " & JobID

        Dim conn As New SqlConnection(ConnString)

        Using cmd As New SqlCommand
            cmd.CommandType = CommandType.Text
            cmd.Connection = conn
            cmd.CommandText = CmdString
            conn.Open()

            Dim reader As SqlDataReader = cmd.ExecuteReader
            While reader.Read()
                JobID = reader.Item(0)
                Survey = reader.Item(1)
                RequestedBy = reader.Item(2)
                Phone = reader.Item(3)
                CompletionDt = reader.Item(4)
                NotesText = reader.Item(5)
                WONum = reader.Item(6)
                RequestDate = reader.Item(7)

                EmailString = "<html>"
                EmailString += "<body>"
                EmailString += "<h2>Equipment Move Request</h2>"
                EmailString += "<p style='font-size:125%;'>"
                EmailString += "<b>Work Order Number: </b>" & WONum & "<br />"
                EmailString += "<b>Requested On: </b>" & RequestDate & "<br />"
                EmailString += "<b>Survey: </b>" & Survey & "<br />"
                EmailString += "<b>Requested By: </b>" & RequestedBy & "<br />"
                If Phone <> "" Then
                    EmailString += "<b>Phone: </b>" & Phone & "<br />"
                Else
                    EmailString += "<b>Phone: </b> None given <br />"
                End If
                EmailString += "<b>Requested Completion Date: </b>" & CompletionDt & " <br /> "
                EmailString += "<b>Notes: </b>" & NotesText.Trim & "</p>"

            End While

            conn.Close()
        End Using

        'Dim EmailString As String = ""
        'Dim MoveTableStartString As String = ""
        'Dim CloseTableString As String = "</table>"
        'Dim SurplusTableStartString As String = ""
        'Dim ItemString = ""
        'Dim PreviousBuilding As String = ""
        'Dim CurrentBuilding As String = ""
        'Dim SectionTitleString As String = ""

        SectionTitleString = "<p style='font-size:125%;'>"
        MoveTableStartString = "<table cellspacing = 0 style='font-size:125%; border-top:1px solid black;'>"
        MoveTableStartString += "<tr>"
        MoveTableStartString += td_formatting_100 + "<b>PTag</b></td>"
        MoveTableStartString += td_formatting_300 + "<b>To</b></td>"
        MoveTableStartString += td_formatting_300 + "<b>Description</b></td>"
        MoveTableStartString += "</tr>"
        SurplusTableStartString = "<table cellspacing = 0 style='font-size:125%; border-top:1px solid black;'>"
        SurplusTableStartString += "<tr>"
        SurplusTableStartString += td_formatting_100 + "<b>PTag</b></td>"
        SurplusTableStartString += td_formatting_300 + "<b>Description</b></td>"
        SurplusTableStartString += "</tr>"



        CmdString = "Select MoveType, ToBuilding, BuildingID, ToRoom, Custodian, "
        CmdString += "PTag, SerialNumber, Description, FromBuilding, FromBuildingID, FromRoom "
        CmdString += "FROM " & Schema & ".MoveItems WHERE JobID = " & JobID & " "
        CmdString += "ORDER BY MoveType, FromBuilding"

        TableString = "<table Class='confirmationTable'><tr>"
        TableString += "<th Class='confirmationTableth'> Move Type </th>"
        TableString += "<th class='confirmationTableth'> From Building</th>"
        TableString += "<th class='confirmationTableth'> From Room</th>"
        TableString += "<th class='confirmationTableth'> To Building </th>"
        TableString += "<th class='confirmationTableth'> To Room </th>"
        TableString += "<th class='confirmationTableth'> Custodian </th>"
        TableString += "<th class='confirmationTableth'> PTag </th>"
        TableString += "<th class='confirmationTableth'> Serial # </th>"
        TableString += "<th class='confirmationTableth'> Description </th>"
        TableString += "</tr>"

        'EmailString += "<br /><b>Move Items</b><br />------------------------<br />"
        Using cmd As New SqlCommand
            cmd.CommandType = CommandType.Text
            cmd.Connection = conn
            cmd.CommandText = CmdString
            conn.Open()

            Dim reader As SqlDataReader = cmd.ExecuteReader
            Dim IsFirstMoveRecord As Boolean = True
            Dim IsFirstSurplusRecord As Boolean = True
            While reader.Read()

                TableString += "<tr class='confirmationTabletr'>"
                'Move Type
                TableString += "<td class='confirmationTabletd' width='100px'>" & reader.Item(0) & "</td>"

                'From Building
                TableString += "<td class='confirmationTabletd' width='350px'>" & reader.Item(8) & "</td>"

                'From Room
                TableString += "<td class='confirmationTabletd' with='50px'>" & reader.Item(10) & "</td>"

                'To Building
                TableString += "<td class='confirmationTabletd' width='350px'>" & reader.Item(1) & "</td>"

                'To Room
                TableString += "<td class='confirmationTabletd' width='50px'>" & reader.Item(3) & "</td>"

                'Custodian
                TableString += "<td class='confirmationTabletd' width='200px'>" & reader.Item(4) & "</td>"

                'PTag
                TableString += "<td class='confirmationTabletd' width='60px'>" & reader.Item(5) & "</td>"

                'Serial #
                TableString += "<td class='confirmationTabletd' width='60px'>" & reader.Item(6) & "</td>"

                'Description
                TableString += "<td class='confirmationTabletd' width='400px'>" & reader.Item(7) & "</td>"
                TableString += "</tr>"

                'F&S asked for certain formatting for equipment move emails. The code below builds it as it
                'cycles through the record. Best way to see how this all shakes out
                If reader.Item(0) <> "Surplus" Then

                    'if this is our first record, set previous building and current building to 
                    'the 'from' building name of the record & turn off "IsFirstRecord"
                    'if not first record, just set current building name to the 'from' building
                    'of the current record
                    If IsFirstMoveRecord Then
                        EmailString += "<h2>--- Items to Move ---</h2>"
                        PreviousBuilding = reader.Item(8)
                        CurrentBuilding = PreviousBuilding
                        EmailString += SectionTitleString + "<b>From: </b>" + CurrentBuilding
                        If reader.Item(10) <> "" Then
                            EmailString += ", Rm: " + reader.Item(10)
                        End If
                        EmailString += "</p>" + MoveTableStartString
                        IsFirstMoveRecord = False
                    Else
                        CurrentBuilding = reader.Item(8)
                    End If

                    'If the buildings are the same, we are still listing move items for the same building
                    'else, we need to end the table for the previous building and start a new table
                    'for the new building
                    If CurrentBuilding <> PreviousBuilding Then
                        EmailString += "</table>"
                        EmailString += SectionTitleString + "<b>From: </b>" + CurrentBuilding
                        If reader.Item(10) <> "" Then
                            EmailString += ", Rm: " + reader.Item(10)
                        End If
                        EmailString += "</p>" + MoveTableStartString
                    End If

                    ItemString = "<tr>"
                    If reader.Item(5) <> "" Then
                        ItemString += td_formatting_100 & reader.Item(5) & "</td>"
                    Else
                        ItemString += td_formatting_100 + "No Ptag"
                    End If

                    ItemString += td_formatting_300 & reader.Item(1)
                    If reader.Item(3) <> "" Then
                        ItemString += ", Rm: " + reader.Item(3)
                    End If
                    ItemString += "</td>"
                    ItemString += td_formatting_300 & reader.Item(7) & "</td>"
                    ItemString += "</tr>"
                    EmailString += ItemString

                ElseIf reader.Item(0) = "Surplus" Then

                    If IsFirstSurplusRecord Then
                        EmailString += "</table>"
                        EmailString += "<h2>--- Items to Surplus ---</h2>"
                        PreviousBuilding = reader.Item(8)
                        CurrentBuilding = PreviousBuilding
                        EmailString += SectionTitleString + "<b>From: </b>" + CurrentBuilding + ", Rm: " + reader.Item(10) + "</p>" + SurplusTableStartString
                        IsFirstSurplusRecord = False
                    Else
                        CurrentBuilding = reader.Item(8)
                    End If
                    If CurrentBuilding <> PreviousBuilding Then
                        EmailString += "</table>"
                        EmailString += SectionTitleString + "<b>From: </b>" + CurrentBuilding + ", Rm: " + reader.Item(10) + "</p>" + SurplusTableStartString
                    End If

                    ItemString = "<tr>"
                    If reader.Item(5) <> "" Then
                        ItemString += td_formatting_100 & reader.Item(5) & "</td>"
                    Else
                        ItemString += td_formatting_100 + "No Ptag"
                    End If

                    ItemString += td_formatting_300 & reader.Item(7) & "</td>"
                    ItemString += "</tr>"
                    EmailString += ItemString
                End If


                PreviousBuilding = CurrentBuilding
            End While

            EmailString += "</table></body></html>"
            conn.Close()
        End Using

        TableString += "</table>"
        SubmissionErrorLabel.Text = ""
        ErrorHeaderLabel.Text = " <div Class='shadedSuccessBox'><div class='confirmationHeader'>Your request has been submitted</div>"
        SurveyLabel.Text = "<b>Survey:</b> " & Survey & "<br />"
        RequestedByLabel.Text = "<b>Requested By: </b>" & RequestedBy & "<br />"
        RequestedOnLabel.Text = "<b>Requested On: </b>" & RequestDate & "<br />"
        WONumberLabel.Text = "<b>Work Order Number: </b>" & WONum & "<br />"

        If PhoneNumber <> "" Then
            PhoneLabel.Text = "<b>Phone:</b>" & Phone & "<br />"
        Else
            PhoneLabel.Text = "<b>Phone:</b> None Provided <br />"
        End If

        CompletionDateLabel.Text = "<b>Requested Completion Date: </b>" & CompletionDt & "<br />"
        NotesLabel.Text = "<b>Notes: </b>" & NotesText & "<br /><br />"
        TableString += "</div>"
        StaticTable.Controls.Add(New LiteralControl(TableString))

        '*****
        'If having trouble with email formatting, uncomment this line. It will allow you to see the email content in a browser,
        'and you can inspect elements to see what might have broken when the email content was being constructed
        'StaticTable.Controls.Add(New LiteralControl(EmailString))
        '******

        SendConfirmationEmail(EmailString)
        SendProcessingEmail(EmailString)
        FormEditableContent.Visible = False
        RestartButton.Visible = True

    End Sub

    Protected Sub SendConfirmationEmail(body As String)
        Try
            Using sw As New StringWriter
                Using hw As New HtmlTextWriter(sw)

                    Dim sr As New StringReader(sw.ToString())

                    Dim mm As New MailMessage()

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

                    If PRIForms.getHost() = "apps.prairie.illinois.edu" Then
                        mm.From = New MailAddress("dblobaum@illinois.edu")
                    Else
                        mm.From = New MailAddress("afodom@illinois.edu")
                    End If

                    mm.Subject = "Received: Equipment Move Request"

                    'To prevent uofi spam filters from flagging any emails sent
                    mm.Headers.Add("X-MessageSource", "Prairie Purchase Request")

                    mm.Body = body

                    mm.IsBodyHtml = True

                    'Need error handling code for email send failure?
                    Try
                        Dim smtp As New SmtpClient()
                        smtp.Host = "express-smtp.cites.uiuc.edu"
                        smtp.Send(mm)
                    Catch ex As Exception
                        MsgBox("Error: Unable to email work request to PRI. " & Server.HtmlEncode(ex.ToString()))
                    End Try

                End Using
            End Using
        Catch ex As Exception
            MsgBox("Error Sending PRI Equipment Move Request Confirmation Email - " & Server.HtmlEncode(ex.ToString()))
        End Try

    End Sub

    Protected Sub SendProcessingEmail(body As String)

        Try
            Using sw As New StringWriter
                Using hw As New HtmlTextWriter(sw)

                    Dim sr As New StringReader(sw.ToString())

                    Dim mm As New MailMessage()

                    If PRIForms.getHost() = "apps.prairie.illinois.edu" Then
                        mm.To.Add("dblobaum@illinois.edu")
                        mm.To.Add("bryant2@illinois.edu")
                        mm.To.Add("dblobaum@illinois.edu")
                        mm.To.Add("rblacker@illinois.edu")
                        mm.To.Add("rpadilla@illinois.edu")
                        mm.To.Add("bma10@illinois.edu")
                        mm.From = New MailAddress("dblobaum@illinois.edu")
                    Else
                        mm.To.Add("afodom@illinois.edu")
                        mm.From = New MailAddress("afodom@illinois.edu")
                    End If

                    mm.Subject = "Action Required: Equipment Move Request"

                    'To prevent uofi spam filters from flagging any emails sent
                    mm.Headers.Add("X-MessageSource", "Prairie Purchase Request")

                    mm.Body = body

                    mm.IsBodyHtml = True

                    'Need error handling code for email send failure?
                    Try
                        Dim smtp As New SmtpClient()
                        smtp.Host = "express-smtp.cites.uiuc.edu"
                        smtp.Send(mm)
                    Catch ex As Exception
                        MsgBox("Error: Unable to email work request to PRI. " & Server.HtmlEncode(ex.ToString()))
                    End Try

                End Using
            End Using
        Catch ex As Exception
            MsgBox("Error Sending PRI Equipment Move Request Confirmation Email - " & Server.HtmlEncode(ex.ToString()))
        End Try
    End Sub
    Public Sub OnDeleteRow(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.GridViewDeleteEventArgs)

        If ViewState("CurrentTable") IsNot Nothing Then
            Dim dt As DataTable = CType(ViewState("CurrentTable"), DataTable)

            dt.Rows.RemoveAt(e.RowIndex)
            RequestGrid.DataSource = dt
            RequestGrid.DataBind()

            ViewState("CurrentTable") = dt

            If dt.Rows.Count() = 0 Then
                MistakeInstructions.Visible = False
            End If
        Else
            Response.Write("ViewState is null in RequestGrid_RowDeleting")
        End If

    End Sub

    Protected Sub LoadTable(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim dr As DataRow
        dr = Nothing

        dt = New DataTable

        dt.Columns.Add(New DataColumn("MoveType", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("FromBuilding", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("FromBuildingID", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("FromRoom", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("ToBuilding", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("ToBuildingID", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("ToRoom", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("PTag", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("Description", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("Custodian", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("SerialNumber", Type.GetType("System.String")))

        'put the current datatable in the viewstate, to hold info through postback
        ViewState("CurrentTable") = dt

        'Tell the gridview that the datatable is its source, and bind it
        RequestGrid.DataSource = dt
        RequestGrid.DataBind()

    End Sub

    Protected Sub getData(ByVal sender As Object, ByVal e As EventArgs)
        Dim RequestorTextBox As TextBox = TryCast(FindControl("RequestedBy"), TextBox)
        Requestor = RequestorTextBox.Text
        SurveyName = SurveyChoice.SelectedItem.Text
        PhoneNumber = Phone.Text
        CompletionDt = CompletionDate.Text
    End Sub

    'Type is which portion of the form to check for blanks. 
    '"item" means user just clicked "Add Item" button, and we need to check item info before adding
    '"job" means user clicked submit button, and we need to check job info before submitting to database
    Function CheckForBlanks(type As String) As List(Of String)
        Dim EmptyFields As New List(Of String)
        Dim HasBlanks As Boolean = False
        Dim RequestorTextBox As TextBox = TryCast(FindControl("RequestedBy"), TextBox)

        If type = "job" Then
            Requestor = RequestorTextBox.Text
            SurveyName = SurveyChoice.SelectedItem.Text
            PhoneNumber = Phone.Text
            CompletionDt = CompletionDate.Text
            If SurveyName = "---Select Survey---" Then
                EmptyFields.Add("Survey Name")
            End If

            If Requestor Is Nothing Or Requestor = "" Then
                EmptyFields.Add("Requested By")
            End If

            If CompletionDt Is Nothing Or CompletionDt = "" Then
                EmptyFields.Add("Requested Completion Date")
            End If
        Else
            If ddlMoveType.SelectedItem.Text <> "To Trash/Surplus" Then

                If ddlMoveType.SelectedItem.Text = "--Select Type--" Then
                    EmptyFields.Add("Move Type")
                End If

                If ddlFromBuilding.SelectedItem.Text = "--Select Building--" Then
                    EmptyFields.Add("From Building")
                End If

                If ddlFromBuilding.SelectedItem.Text <> "Surplus" Then
                    If tbFromRoom.Text = "" Then
                        EmptyFields.Add("From Room Number")
                    End If
                End If

                If ddlToBuilding.SelectedItem.Text = "--Select Building--" Then
                        EmptyFields.Add("To Building")
                    End If

                    If tbRoomNumber.Text Is Nothing Or tbRoomNumber.Text = "" Then
                        EmptyFields.Add("To Room Number")
                    End If

                    If acCustodian.Text Is Nothing Or acCustodian.Text = "" Then
                        EmptyFields.Add("Responsible Person Post-Move")
                    End If
                End If

                If tbDescription.Text Is Nothing Or tbDescription.Text = "" Then
                EmptyFields.Add("Description")
            End If

        End If

        Return EmptyFields

    End Function

End Class





