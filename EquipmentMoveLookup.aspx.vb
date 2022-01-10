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
Partial Class Default2
    Inherits System.Web.UI.Page

    Dim dt As DataTable
    Dim dtItems As DataTable

    Dim ServiceStatusList As New List(Of String)({"Not Started", "In Progress", "Complete"})
    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load

        UpdateConfirmed.Visible = "False"

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")
        Dim QueryString As String
        Dim dr As DataRow = Nothing

        LoadTable(sender, e)

        QueryString = "select id, Survey, RequestedBy, "
        QueryString += "FORMAT(RequestedCompletionDate,'MM/dd/yyyy') as RequestedCompletionDate, ServiceStatus, "
        QueryString += "FORMAT(CompletionDate, 'MM/dd/yyyy'), WONumber, "
        QueryString += "FORMAT(RequestDate, 'MM/dd/yyyy') "
        QueryString += "FROM " & Schema & ".MoveJob ORDER BY WONumber DESC"

        Dim Conn As New SqlConnection(ConnString)
        Conn.Open()
        Dim dataReader As SqlDataReader
        Dim cmd = New SqlCommand(QueryString, Conn)
        dataReader = cmd.ExecuteReader()

        While dataReader.Read
            dr = dt.NewRow
            dr("ID") = dataReader.Item(0)
            dr("Survey") = dataReader.Item(1)
            dr("RequestedBy") = dataReader.Item(2)
            dr("RequestedCompletionDate") = dataReader.Item(3)
            dr("ServiceStatus") = dataReader.Item(4)
            dr("ActualCompletionDate") = dataReader.Item(5)
            dr("WONumber") = dataReader.Item(6)
            dr("RequestDate") = dataReader.Item(7)
            dt.Rows.Add(dr)
        End While

        RecordGrid.Columns(0).Visible = True
        RecordGrid.DataSource = dt
        RecordGrid.DataBind()

        ViewState("RecordTable") = dt

        RecordGrid.Columns(0).Visible = False

    End Sub
    Protected Sub RecordGrid_SelectedIndexChanged(sender As Object, e As EventArgs)
        RecordList.Visible = False
        SingleRecord.Visible = True

        Dim CurrentRow As GridViewRow = RecordGrid.SelectedRow

        Dim ConnString As String = PRIForms.getConnectionInfo("connection")
        Dim Schema As String = PRIForms.getConnectionInfo("schema")

        dt = ViewState("RecordTable")

        Dim QueryString = "select id, Survey, RequestedBy, Phone, WONumber, "
        QueryString += "FORMAT(RequestDate,'MM/dd/yyyy') as RequestDate, "
        QueryString += "FORMAT(RequestedCompletionDate,'MM/dd/yyyy') as RequestedCompletionDate, Notes, "
        QueryString += "ServiceStatus, FORMAT(CompletionDate,'MM/dd/yyyy'), ServiceNotes "
        QueryString += "FROM " & Schema & ".MoveJob "
        QueryString += "WHERE ID = '" & RecordGrid.SelectedRow.Cells(0).Text & "'"

        Dim Conn As New SqlConnection(ConnString)
        Conn.Open()

        Dim dataReader As SqlDataReader
        Dim cmd = New SqlCommand(QueryString, Conn)
        dataReader = cmd.ExecuteReader()
        While dataReader.Read()
            RequestID.Text = RecordGrid.SelectedRow.Cells(0).Text
            Survey.Text = dataReader(1)
            RequestedBy.Text = dataReader(2)
            If dataReader(3) <> Nothing And dataReader(3) <> "" Then
                Phone.Text = dataReader(3)
            Else
                Phone.Text = "No phone number provided"
            End If
            If IsDBNull(dataReader(4)) Then
                WONumber.Text = "Unavailable"
            Else
                WONumber.Text = dataReader(4)
            End If

            If IsDBNull(dataReader(5)) Then
                RequestDate.Text = "Unavailable"
            Else
                RequestDate.Text = dataReader(5)
            End If

            RequestedCompletionDate.Text = dataReader(6)
            If Not IsDBNull(dataReader(7)) Then
                RequestorNotes.Text = dataReader(7)
            End If

            If Not IsDBNull(dataReader(8)) Then
                ddlStatus.SelectedValue = dataReader(8)
            End If
            If Not IsDBNull(dataReader(9)) Then
                CompletionDate.Text = dataReader(9)
            End If
            If Not IsDBNull(dataReader(10)) Then
                tbServiceNotes.Text = dataReader(10)
            End If

        End While
        dataReader.Close()
        cmd.Dispose()
        Conn.Close()

        dtItems = New DataTable

        dtItems.Columns.Add(New DataColumn("MoveType", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("FromBuilding", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("FromRoom", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("ToBuilding", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("ToRoom", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("Custodian", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("PTag", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("SerialNumber", Type.GetType("System.String")))
        dtItems.Columns.Add(New DataColumn("Description", Type.GetType("System.String")))

        'Tell the gridview that the datatable is its source, and bind it
        ItemGrid.DataSource = dtItems
        ItemGrid.DataBind()

        QueryString = "SELECT MoveType, ToBuilding, ToRoom, Custodian, PTag, SerialNumber, Description, FromBuilding, FromRoom  "
        QueryString += "FROM " & Schema & ".MoveItems "
        QueryString += "WHERE JobID = '" & RequestID.Text & "'"

        Conn.Open()
        cmd = New SqlCommand(QueryString, Conn)
        dataReader = cmd.ExecuteReader()
        Dim dr As DataRow
        While dataReader.Read
            dr = dtItems.NewRow
            dr("MoveType") = dataReader.Item(0)
            If dataReader(1) = "Trash/Surplus" Then
                dr("ToBuilding") = "Trash/Surplus"
            Else
                dr("ToBuilding") = dataReader(1)
            End If
            dr("ToRoom") = dataReader.Item(2)
            dr("Custodian") = dataReader.Item(3)
            dr("PTag") = dataReader.Item(4)
            dr("SerialNumber") = dataReader.Item(5)
            dr("Description") = dataReader.Item(6)
            dr("FromBuilding") = dataReader(7)
            dr("FromRoom") = dataReader(8)
            dtItems.Rows.Add(dr)
        End While

        ItemGrid.DataSource = dtItems
        ItemGrid.DataBind()

        ddlStatus.DataSource = ServiceStatusList
        ddlStatus.DataBind()


    End Sub

    Protected Sub ddlStatus_SelectedIndexChanged(ByVal sender As Object, ByVal e As EventArgs)
        If ddlStatus.SelectedValue = "Complete" Then
            CompletionDateSection.Visible = True
        Else
            CompletionDate.Text = ""
        End If
    End Sub

    Protected Sub Update_Button(ByVal sender As Object, ByVal e As EventArgs) Handles UpdateRecord.Click

        Dim ConnString As String
        Dim UpdateString As String

        Dim ServiceStatus As String = ddlStatus.SelectedValue

        Dim CompletionDt As String
        If Not IsDBNull(CompletionDate.Text) And CompletionDate.Text IsNot Nothing And CompletionDate.Text <> "" Then
            CompletionDt = CompletionDate.Text
        Else
            CompletionDt = ""
        End If

        Dim ServiceNotes As String = tbServiceNotes.Text

        If PRIForms.getHost() = "apps.prairie.illinois.edu" Then
            ConnString = ConfigurationManager.ConnectionStrings("DatastormConnStr").ConnectionString
            UpdateString = "UPDATE Forms.MoveJob "
        Else
            ConnString = ConfigurationManager.ConnectionStrings("HailConnStr").ConnectionString
            UpdateString = "UPDATE dbo.MoveJob "
        End If

        UpdateString += "SET ServiceStatus = '" & ServiceStatus & "', "
        If CompletionDt <> "" Then
            UpdateString += "CompletionDate = '" & CompletionDt & "', "
        End If
        UpdateString += "ServiceNotes = '" & ServiceNotes & "' "
        UpdateString += "WHERE ID = '" & RequestID.Text & "'"


        Dim Conn As New SqlConnection(ConnString)
        Dim rows As Integer
        Conn.Open()
        Dim cmd = New SqlCommand(UpdateString, Conn)
        rows = cmd.ExecuteNonQuery()
        Conn.Close()
        cmd.Dispose()

        UpdateConfirmed.Visible = True

    End Sub

    Protected Sub ReturnToList_Button(ByVal sender As Object, ByVal e As EventArgs) Handles ReturnToList.Click
        Response.Redirect("EquipmentMoveLookup.aspx")
    End Sub

    Protected Sub BackToDashboard_Button(ByVal sender As Object, ByVal e As EventArgs) Handles BackToDashboard.Click
        Response.Redirect("Default.aspx")
    End Sub

    Protected Sub LoadTable(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim dr As DataRow
        dr = Nothing

        dt = New DataTable

        dt.Columns.Add(New DataColumn("ID", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("Survey", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("RequestedBy", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("FromBuilding", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("FromRoom", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("RequestedCompletionDate", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("ServiceStatus", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("ActualCompletionDate", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("WONumber", Type.GetType("System.String")))
        dt.Columns.Add(New DataColumn("RequestDate", Type.GetType("System.String")))

        'put the current datatable in the viewstate, to hold info through postback
        ViewState("CurrentTable") = dt

        'Tell the gridview that the datatable is its source, and bind it
        RecordGrid.DataSource = dt
        RecordGrid.DataBind()

    End Sub
End Class
