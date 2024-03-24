Imports Microsoft.VisualBasic

Public Class ngPnPrilog

    Public Property id As Integer
    Public Property pn_id As Integer
    Public Property redbr As Integer
    Public Property opis As String
    Public Property iznos_val As Decimal?
    Public Property iznos As Decimal?
    Public Property kategorija As Integer?
    Public Property pn_obracun_id As Integer?
    Public Property obracun As String
    Public Property odobren As Boolean
    Public Property napomena As String
    Public Property timestamp As DateTime

End Class

Public Class LogEntry
    Public Property Details As String
    Public Property LogDate As DateTime
End Class