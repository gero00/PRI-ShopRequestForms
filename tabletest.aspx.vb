
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

    Shared dt As New DataTable
    Shared ddl As New DropDownList
    Shared ddl_list As New List(Of String)({"--Select Type--", "Move/Transfer", "Trash/Surplus"})



    Protected Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Me.Load
        'Response.Write("Page_Load event")

        Dim dr As DataRow
        dr = Nothing

        dt = New DataTable

        If Not (IsPostBack) Then
            dt.Clear()

            'Create a table with three columns. "Row" column will hold an auto-generated row number.
            'Other columns will hold text boxes, though we tell them type string here. I don't
            'understand this completely but it works
            dt.Columns.Add(New DataColumn("Row", Type.GetType("System.String")))
            dt.Columns.Add(New DataColumn("Column1", Type.GetType("System.String")))
            dt.Columns.Add(New DataColumn("Column2", Type.GetType("System.String")))
            dt.Columns.Add(New DataColumn("Column3", Type.GetType("System.String")))

            'set row to 1 -> the number we want to start with for automatic row numbering
            'set columns that will hold text boxes to empty strings. Why does this work?
            dr = dt.NewRow()
            dr("Row") = 1
            dr("Column1") = String.Empty
            dr("Column2") = String.Empty
            dr("Column3") = String.Empty

            'add the row to the datatable
            dt.Rows.Add(dr)

            'put the current datatable in the viewstate, to hold info through postback
            ViewState("CurrentTable") = dt

            'Tell the gridview that the datatable is its source, and bind it
            RequestGrid.DataSource = dt
            RequestGrid.DataBind()

            'populate the dropdown list with the data in ddl_list
            ddl = CType(RequestGrid.Rows(0).Cells(1).FindControl("DDL"), DropDownList)
            ddl.DataSource = ddl_list
            ddl.DataBind()

        End If

    End Sub

    'saves entered contents into datatable in view state, so it will 
    'persist through post-back
    Protected Sub SavePreviousData()

        'will be used to count rows
        Dim rowindex As Integer = 0

        'only do this if we have something in the viewstate for this table
        If ViewState("CurrentTable") IsNot Nothing Then
            'cast the viewstate element as a datatable and put in
            'global datatable variable

            'only to this if there are rows in the datatable
            If dt.Rows.Count > 0 Then

                'for every row in the table we pulled from the view state, 
                'extract the values entered in the textboxes and put their values into the 
                'datatable textboxes and dropdownlist. The controls are named in the aspx code
                For i As Integer = 1 To dt.Rows.Count - 1
                    'extract the control values
                    Dim box1 As TextBox = CType(RequestGrid.Rows(rowindex).Cells(1).FindControl("TB1"), TextBox)
                    Dim box2 As TextBox = CType(RequestGrid.Rows(rowindex).Cells(1).FindControl("TB2"), TextBox)
                    Dim ddl1 As DropDownList = CType(RequestGrid.Rows(rowindex).Cells(1).FindControl("DDL"), DropDownList)

                    'store the values in the datatable
                    box1.Text = dt.Rows(i).Item("Column1").ToString()
                    box2.Text = dt.Rows(i).Item("Column2").ToString()
                    ddl1.SelectedValue = dt.Rows(i).Item("Column3")

                    rowindex = rowindex + 1

                Next

                'store the datatable in the view state to persist through postback
                ViewState("CurrentTable") = dt
            End If

        Else
            Response.Write("RequestGrid viewstate is null Integer SetPreviousData()")

        End If
    End Sub
    Protected Sub Add_Button(ByVal sender As Object, ByVal e As EventArgs) Handles add.Click

        'only do this if we have something in the viewstate for this table
        If ViewState("CurrentTable") IsNot Nothing Then


            Dim dr As DataRow
            dr = Nothing

            'will be used to count rows
            Dim rowIndex As Integer = 0

            'cast the viewstate element as a datatable and put in
            'global datatable variable
            dt = TryCast(ViewState("CurrentTable"), DataTable)

            'only do this if there are rows in the datatable already
            If dt.Rows.Count > 0 Then

                'for every row in the table we pulled from the view state, 
                'extract the values entered in the textboxes and put their values into the 
                'datatable controls. The controls are named in the aspx code

                For i As Integer = 1 To dt.Rows.Count
                    Dim box1 As TextBox = CType(RequestGrid.Rows(rowIndex).Cells(1).FindControl("TB1"), TextBox)
                    Dim box2 As TextBox = CType(RequestGrid.Rows(rowIndex).Cells(1).FindControl("TB2"), TextBox)
                    Dim ddl1 As DropDownList = CType(RequestGrid.Rows(rowIndex).Cells(1).FindControl("DDL"), DropDownList)

                    dr = dt.NewRow()
                    dr("Row") = i + 1
                    dr("Column1") = box1.Text
                    dr("Column2") = box2.Text
                    dr("Column3") = ddl1.SelectedValue

                    rowIndex = rowIndex + 1
                Next
            End If

            'add the row to the datatable
            dt.Rows.Add(dr)

            'add datatable to viewstate so it will persist through postback
            ViewState("CurrentTable") = dt

            'bind datatable to gridview as datasource
            'this puts everything we have stored in the table so far onto the screen
            'also adds new row (is that just a feature of datagrid?)
            RequestGrid.DataSource = dt
            RequestGrid.DataBind()

            'populate dropdown list with choices from ddl_list.
            'need to do for every single row in the table.
            For i As Integer = 0 To dt.Rows.Count - 1
                ddl = CType(RequestGrid.Rows(i).Cells(1).FindControl("DDL"), DropDownList)
                ddl.DataSource = ddl_list
                ddl.DataBind()
            Next

        Else
            Response.Write("RequestGrid viewstate is null in Add_Button")

        End If

        'Save previous data so we will have it on post back
        SavePreviousData()


    End Sub
End Class
