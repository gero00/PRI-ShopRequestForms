Imports System.Data
Imports System.Data.SqlClient
Imports System.Drawing
Partial Class ShopLookup
    Inherits System.Web.UI.Page

    Dim dt As DataTable
    Dim ServiceStatusList As New List(Of String)({"Old Data", "Not Started", "In Progress", "Complete"})

    'variables to hold information for account dropdown
    Shared fopa As List(Of String) = New List(Of String)()
    Shared fop_pi As List(Of String) = New List(Of String)()

    'variables for PI emails that are used when sending email notifications
    Shared CFOP_proxy_email_str As String = "" 'Placeholder String listing all proxies for funds included in purchase request
    Shared CFOP_proxy_emails As New List(Of String)() 'CFOP_proxy_emails list contains the emails for all CFOP PI proxies for a particular fund. Used to populate ddl_CFOP_emails dropdown to notify for approval


    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        'on first page load, make sure update comfirmation is not visible
        UpdateConfirmed.Visible = "False"

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")
        Dim QueryString As String
        dt = New DataTable

        'Get data that needs to be displayed in the RecordGrid control
        QueryString = "SELECT ID, WONumber, RequestedBy, FORMAT(RequestedCompletionDate,'MM/dd/yyyy') as RequestedCompletionDate, "
        QueryString += "FORMAT(RequestDate,'MM/dd/yyyy') as RequestDate, "
        QueryString += "ServiceStatus, "
        QueryString += "CASE WHEN CompletionDate = '1900-01-01' THEN '' ELSE "
        QueryString += "FORMAT(CompletionDate, 'MM/dd/yyy') END AS ActualCompletionDate, "
        QueryString += "CASE when len(Description)>=140 then left(Description, 140) + '...' Else Description End as Description "
        QueryString += "FROM " & Schema & ".ShopRequest ORDER BY WONumber DESC"

        Dim Conn As New SqlConnection(ConnString)
        Conn.Open()
        Dim dataReader As SqlDataReader
        Dim cmd = New SqlCommand(QueryString, Conn)
        dataReader = cmd.ExecuteReader()

        'load query results into datatable
        'bind data table to record grid
        dt.Load(dataReader)
        RecordGrid.Columns(0).Visible = True
        RecordGrid.DataSource = dt
        RecordGrid.DataBind()

        'put the datatable in the viewstate, in case we needed it across sessions
        ViewState("RecordTable") = dt

        RecordGrid.Columns(0).Visible = False

    End Sub

    'when user selects record, show information for specific record along with update fields
    Protected Sub RecordGrid_SelectedIndexChanged(sender As Object, e As EventArgs)

        'hide the full record list, make the single record block visible
        RecordList.Visible = False
        SingleRecord.Visible = True

        ddlStatus.DataSource = ServiceStatusList
        ddlStatus.DataBind()
        ddlStatus.Items(0).Enabled = False

        Dim CurrentRow As GridViewRow = RecordGrid.SelectedRow

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")

        'refill datatable from informaiton stored in viewstate
        dt = ViewState("RecordTable")

        'get all information related to selected record
        Dim QueryString = "SELECT ID, RequestedBy, "
        QueryString += "FORMAT(RequestedCompletionDate,'MM/dd/yyyy') RequestedCompletionDate,  "
        QueryString += "Account, AccountPI, Description, ServiceStatus, "
        QueryString += "CASE WHEN CompletionDate = '1900-01-01' THEN '' ELSE "
        QueryString += "FORMAT(CompletionDate, 'MM/dd/yyy') END AS CompletionDate, ServiceNotes, "
        QueryString += "WONumber,  FORMAT(RequestDate,'MM/dd/yyyy') as RequestDate, FundManager, Justification "
        QueryString += "FROM " & Schema & ".ShopRequest "
        QueryString += "WHERE ID = '" & RecordGrid.SelectedRow.Cells(0).Text & "'"

        Dim Conn As New SqlConnection(ConnString)
        Conn.Open()

        Dim dataReader As SqlDataReader
        Dim cmd = New SqlCommand(QueryString, Conn)
        dataReader = cmd.ExecuteReader()

        'put all information for this record in appropriate locations on the page
        While dataReader.Read()
            RequestID.Text = RecordGrid.SelectedRow.Cells(0).Text
            RequestedBy.Text = dataReader(1)
            RequestedCompletionDate.Text = dataReader(2)
            txt_CFOP.Text = dataReader(3)
            PI.Text = dataReader(4)
            Description.Text = dataReader(5)
            If Not IsDBNull(dataReader(6)) Then
                'For items created before status option  was created, the status will be empty
                'Set it to "Old Data" until F&S user changes it
                If dataReader(6) = "" Then
                    ddlStatus.SelectedValue = "Old Data"
                Else
                    ddlStatus.SelectedValue = dataReader(6)
                End If
                'Only show completion date if the record is marked complete
                If ddlStatus.SelectedValue = "Complete" Then
                    CompletionDateSection.Visible = True
                End If
            End If

            If Not IsDBNull(dataReader(7)) Then
                'An empty date field returns 01/01/1900. If that's the case, set completion date to empty.
                If dataReader(7) = "01/01/1900" Then
                    CompletionDate.Text = ""
                Else
                    CompletionDate.Text = dataReader(7)
                End If
            End If

            'only try to put data into the service notes, WO, and Request date fields
            'if there is information in the database
            If Not IsDBNull(dataReader(8)) Then
                tbServiceNotes.Text = dataReader(8)
            End If
            If Not IsDBNull(dataReader(9)) Then
                WONumber.Text = dataReader(9)
            End If
            If Not IsDBNull(dataReader(10)) Then
                RequestedOn.Text = dataReader(10)
            End If
            If Not IsDBNull(dataReader(11)) Then
                FundMgr.Text = dataReader(11)
            End If
            If Not IsDBNull(dataReader(12)) Then
                Justification.Text = dataReader(12)
            End If

        End While
        dataReader.Close()
        cmd.Dispose()

    End Sub

    'when complete is selected from the status field, date field is made visible for user to fill
    Protected Sub ddlStatus_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        If ddlStatus.SelectedValue = "Complete" Then
            CompletionDateSection.Visible = True
        Else
            CompletionDate.Text = ""
        End If
    End Sub

    'handler for when user submits udpate button
    Protected Sub Update_Button(ByVal sender As Object, ByVal e As EventArgs) Handles UpdateRecord.Click

        Dim Account As String = txt_CFOP.Text
        Dim AccountPI As String = PI.Text
        Dim ServiceStatus As String = ddlStatus.SelectedValue
        Dim CompletionDt As String
        Dim ServiceNotes As String = tbServiceNotes.Text
        Dim AccountFM As String = FundMgr.Text

        'If completion date is not null and is not empty AND if service status has a value other than complete
        'Then completion date needs to be set to empty. This handles situations where the item goes from complete
        'back to another status. When that happens, completion date needs to be cleared.
        If Not IsDBNull(CompletionDate.Text) And CompletionDate.Text IsNot Nothing And CompletionDate.Text <> "" Then
            If ServiceStatus = "Not Started" Or ServiceStatus = "In Progress" Or ServiceStatus = "Old Data" Then
                CompletionDt = ""
            Else
                CompletionDt = CompletionDate.Text
            End If

        Else
            CompletionDt = ""
        End If

        If tbServiceNotes.Text.IndexOf("'") = -1 Then
            ServiceNotes = tbServiceNotes.Text
        Else
            ServiceNotes = tbServiceNotes.Text.Replace("'", "''")
        End If

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")
        Dim UpdateString As String

        'Update data in database from info in form fields
        UpdateString = "UPDATE " & Schema & ".ShopRequest "
        UpdateString += "SET ServiceStatus = '" & ServiceStatus & "', "
        UpdateString += "CompletionDate = '" & CompletionDt & "', "
        UpdateString += "ServiceNotes = '" & ServiceNotes & "', "
        UpdateString += "Account = '" & Account & "', "
        UpdateString += "AccountPI = '" & AccountPI & "', "
        UpdateString += "FundManager = '" & AccountFM & "' "
        UpdateString += "WHERE ID = '" & RequestID.Text & "'"

        Dim Conn As New SqlConnection(ConnString)
        Dim rows As Integer
        Conn.Open()
        Dim cmd = New SqlCommand(UpdateString, Conn)
        rows = cmd.ExecuteNonQuery()
        Conn.Close()
        cmd.Dispose()

        'If update is successful, make confirmation message visible
        UpdateConfirmed.Visible = True

    End Sub

    'sends user back to main page that displays all records.
    Protected Sub ReturnToList_Button(ByVal sender As Object, ByVal e As EventArgs) Handles ReturnToList.Click
        Response.Redirect("ShopLookup.aspx")
    End Sub

    'event handler for CFOP field changing in Funding Information grid. Populate PI textbox and notification email dropdown based on fund
    'code taken from purchase form, written by Greg Rogers
    'not totally sure what specifically is happening in all aspects of this code, but it works.
    Public Sub txt_CFOP_TextChanged(sender As Object, e As EventArgs)
        Dim cfop_index As Integer
        Try

            Dim i As Integer = 0 '= sender.EditIndex
            Dim cfop_txt As String = txt_CFOP.Text

            'if a string has been entered in the cfop textbox populate autoextender dropdown
            If cfop_txt <> Nothing And cfop_txt <> "" Then
                cfop_index = fopa.IndexOf(cfop_txt)
                If cfop_txt.Count > 25 Then
                    cfop_txt = cfop_txt.Substring(0, 26)
                    txt_CFOP.Text = cfop_txt
                End If

                If cfop_index <> Nothing And cfop_index <> -1 Then
                    If fop_pi.Count <> 0 And fop_pi.Count > cfop_index Then
                        If fop_pi(cfop_index) <> Nothing Then
                            PI.Text = fop_pi(cfop_index)
                            FundMgr.Text = PRIForms.getFundManager(cfop_txt)
                        Else
                            PI.Text = ""
                        End If
                    Else
                        PI.Text = ""
                    End If
                Else
                    If cfop_txt.Count > 25 Then
                        SearchCFOPS(cfop_txt, 1)
                        If fop_pi.Count <> 0 And fop_pi.Count > cfop_index And cfop_index >= 0 Then
                            If fop_pi(cfop_index) <> Nothing Then
                                PI.Text = fop_pi(cfop_index)
                                FundMgr.Text = PRIForms.getFundManager(cfop_txt)
                            Else
                                PI.Text = ""
                            End If
                        Else
                            PI.Text = ""
                        End If
                    End If
                End If
                GetPIEmail(cfop_txt)

            End If

            If PI.Text = "" Then
                SearchCFOPS(cfop_txt, 1)
                If fop_pi.Count <> 0 Then
                    PI.Text = fop_pi(0)
                    FundMgr.Text = PRIForms.getFundManager(cfop_txt)
                Else
                    PI.Text = "No PI found. Please enter valid CFOPA."
                    FundMgr.Text = ""
                    PI.ForeColor = System.Drawing.Color.Red
                End If
            End If

            PI.Enabled = False
            PI.BackColor = Color.White
            PI.ForeColor = Color.Black

            FundMgr.Enabled = False
            FundMgr.BackColor = Color.White
            FundMgr.ForeColor = Color.Black

        Catch ex As Exception
            MsgBox("Error acquiring CFOP PI: " & ex.ToString() & " fop_pi count: " & fop_pi.Count & " index: " & cfop_index.ToString())
            System.Diagnostics.Debug.Print("Error acquiring CFOP PI: " & ex.ToString())
        End Try

    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ' The asp:AutoCompleteExtender AJAX control integrated into the txtCFOP textbox in the GridviewFund gridview consumes the WebService SearchCFOPS. 
    ' This webservice calls the stored procedure spPRIDEQueryPurchaseOrderCFOP which uses view vwPRIDE_Purchreq_Fund_Info to get the CFOP, PI and PIEmail 
    ' fields and populate PI Fund Grid. There is also an event handler, txt_CFOP_TextChanged, for the txt_CFOP control's OnTextChanged event that also
    ' calls Search_CFOPS in case the user does not select a menu item from the generated dropdownlist and types or pastes the CFOP into the textbox directly.

    'load the CFOP field in the Funding Information grid using the AutoCompleteExtender control
    <System.Web.Script.Services.ScriptMethod(), System.Web.Services.WebMethod()>
    Public Shared Function SearchCFOPS(ByVal prefixText As String, ByVal count As Integer) As List(Of String)

        fopa.Clear()
        fop_pi.Clear()

        Dim CFOPConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString
        Dim CFOPConn As New SqlConnection(CFOPConnStr)

        Dim cmd As New SqlCommand
        Dim dataReader As SqlDataReader

        CFOPConn.Open()
        cmd.Connection = CFOPConn
        cmd.CommandType = CommandType.StoredProcedure
        cmd.CommandText = "spPRIDEQueryPurchaseOrderCFOP" '"select CFOP, PI and PIEmail from Customers where" & " CFOP like @SearchText + '%'"
        cmd.Parameters.Add(New System.Data.SqlClient.SqlParameter("@CFOP", System.Data.SqlDbType.VarChar, 30))
        cmd.Parameters("@CFOP").Value = prefixText + "%"

        dataReader = cmd.ExecuteReader()

        While dataReader.Read
            'MsgBox("Fund Found for search: " & prefixText)
            'fopa.Add(sqldr("CFOP").ToString)
            fopa.Add(dataReader("CFOP").ToString & " --" & dataReader("ActivityDesc").ToString)
            fop_pi.Add(dataReader("PI").ToString)

        End While
        CFOPConn.Close()
        'Dim cstringsettings As System.Configuration.ConnectionStringSettings
        'Dim rootWebConfig As Configuration

        'rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/PRI")


        'cstringsettings = rootWebConfig.ConnectionStrings.ConnectionStrings("ConnectionString2")
        Return fopa
    End Function

    'Get emails for all proxies for all funds included in purchase and populate CFOP_pi_proxy_emails list and CFOP_proxy_email_str string variables and populate the dropdownlist of proxy contacts in the GridViewFund gridview
    Protected Sub GetPIEmail(cfop_str As String)

        CFOP_proxy_email_str = ""
        Dim CFOP_str_all As String = ""
        Dim ConnStr As String = ConfigurationManager.ConnectionStrings("SnoopyConnStr").ConnectionString

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
            sql_get_proxy_emails += " And (fiscalyear = " & current_fiscal_year.ToString() & " Or fiscalyear = 0);"

            cmd = New SqlCommand(sql_get_proxy_emails, objConnection)
            CFOP_proxy_emails.Add(cmd.ExecuteScalar)

            cmd.Dispose()
            objConnection.Close()

        Catch ex As Exception
            MsgBox("Error determining proxies to populate dropdownlist: " & Server.HtmlEncode(ex.ToString()))
            System.Diagnostics.Debug.Print("Error determining proxies to populate dropdownlist: " & ex.ToString())
        End Try
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

    'takes f&s user back to page with all lookup forms linked
    Protected Sub BackToDashboard_Button(ByVal sender As Object, ByVal e As EventArgs) Handles BackToDashboard.Click
        Response.Redirect("Default.aspx")
    End Sub

    'sets up the pager links under the table of records
    Protected Sub RecordGrid_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        RecordGrid.PageIndex = e.NewPageIndex
        RecordGrid.DataSource = dt
        RecordGrid.DataBind()
    End Sub

End Class
