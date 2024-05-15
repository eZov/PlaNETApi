Imports Microsoft.VisualBasic

Public Class ngApiVersion

    Public Property version As String = "0.9.24.05.15"
    Public Property footer As String = "PlaNET | Zov Consulting | ver "
    Public Property mainText As String = "PlaNET - API ver "


    Public Function getFooter() As String

        Return DateTime.Now.Year.ToString() + "-" + footer + version

    End Function

    Public Function getMainText() As String

        Return mainText + version

    End Function

    Public Function getVersion() As String

        Return version

    End Function
End Class
