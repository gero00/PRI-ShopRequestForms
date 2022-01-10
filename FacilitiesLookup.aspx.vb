Imports System.Data
Imports System.Data.SqlClient
Partial Class FacilitiesLookup
    Inherits System.Web.UI.Page

    Dim dt As DataTable
    Dim ServiceStatusList As New List(Of String)({"Old Data", "Not Started", "In Progress", "Complete"})
    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        'on first page load, make sure update comfirmation is not visible
        UpdateConfirmed.Visible = "False"

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")
        Dim QueryString As String
        dt = New DataTable

        'Get data that needs to be displayed in the RecordGrid control
        QueryString = "SELECT id, WONumber, Requestedby, FORMAT(RequestDate,'MM/dd/yyyy') AS RequestDate, "
        QueryString += "FORMAT(RequestedCompletionDate,'MM/dd/yyyy') as RequestedCompletionDate, ServiceStatus, "
        QueryString += "CASE WHEN CompletionDate = '1900-01-01' THEN '' ELSE "
        QueryString += "FORMAT(CompletionDate, 'MM/dd/yyy') END AS ActualCompletionDate, "
        QueryString += "CASE when len(Description)>=140 then left(Description, 140) + '...' Else Description End as Description "
        QueryString += "FROM " & Schema & ".FacilitiesWorkRequest ORDER BY WONumber DESC"

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

        Dim CurrentRow As GridViewRow = RecordGrid.SelectedRow

        'refill datatable from informaiton stored in viewstate
        dt = ViewState("RecordTable")

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")

        'get all information related to selected record
        Dim QueryString = "SELECT id, requestedby, CONCAT(BuildingID, ' - ', Building) as Building, RoomNumber,  "
        QueryString += "FORMAT(RequestedCompletionDate,'MM/dd/yyyy') as RequestedCompletionDate, Description, "
        QueryString += "ServiceStatus, "
        QueryString += "CASE WHEN CompletionDate = '1900-01-01' THEN '' ELSE "
        QueryString += "FORMAT(CompletionDate, 'MM/dd/yyy') END AS CompletionDate, "
        QueryString += "ServiceNotes, WONumber, "
        QueryString += "FORMAT(RequestDate, 'MM/dd/yyyy') as RequestDate "
        QueryString += "FROM " & Schema & ".FacilitiesWorkRequest "
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
            Building.Text = dataReader(2)
            RoomNumber.Text = dataReader(3)
            RequestedCompletionDate.Text = dataReader(4)
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
                If dataReader(6) = "Complete" Then
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
                WONum.Text = dataReader(9)
            End If
            If Not IsDBNull(dataReader(10)) Then
                RequestedOn.Text = dataReader(10)
            End If

        End While
        dataReader.Close()
        cmd.Dispose()

        'why are we rebinding here? Check it out.
        ddlStatus.DataSource = ServiceStatusList
        ddlStatus.DataBind()
        ddlStatus.Items(0).Enabled = False

    End Sub

    'handler for when user submits udpate button
    Protected Sub Update_Button(ByVal sender As Object, ByVal e As EventArgs) Handles UpdateRecord.Click

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")
        Dim UpdateString As String

        Dim ServiceStatus As String = ddlStatus.SelectedValue

        Dim CompletionDt As String

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

        Dim ServiceNotes As String = tbServiceNotes.Text

        'Update data in database from info in form fields
        UpdateString = "UPDATE " & Schema & ".FacilitiesWorkRequest "
        UpdateString += "SET ServiceStatus = '" & ServiceStatus & "', "
        UpdateString += "CompletionDate = '" & CompletionDt & "', "
        UpdateString += "ServiceNotes = '" & ServiceNotes & "' "
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

    'when complete is selected from the status field, date field is made visible for user to fill
    Protected Sub ddlStatus_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        If ddlStatus.SelectedValue = "Complete" Then
            CompletionDateSection.Visible = True
        Else
            CompletionDate.Text = ""
        End If
    End Sub

    'sends user back to main page that displays all records.
    Protected Sub ReturnToList_Button(ByVal sender As Object, ByVal e As EventArgs) Handles ReturnToList.Click
        Response.Redirect("FacilitiesLookup.aspx")
    End Sub

    'sends user back to the page where they can select any lookup page or request form
    Protected Sub BackToDashboard_Button(ByVal sender As Object, ByVal e As EventArgs) Handles BackToDashboard.Click
        Response.Redirect("Default.aspx")
    End Sub

    'sets up the numbered, next, previous controls at bottom of record grid
    Protected Sub RecordGrid_PageIndexChanging(sender As Object, e As GridViewPageEventArgs)
        RecordGrid.PageIndex = e.NewPageIndex
        RecordGrid.DataSource = dt
        RecordGrid.DataBind()
    End Sub

End Class
