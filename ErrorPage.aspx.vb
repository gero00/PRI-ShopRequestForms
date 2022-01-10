
Partial Class ErrorPage
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As System.Object, ByValue As System.EventArgs) Handles Me.Load
        errorMessage.Text = PRIForms.getErrorMessage

    End Sub
End Class
