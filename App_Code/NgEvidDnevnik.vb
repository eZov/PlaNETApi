Imports Microsoft.VisualBasic

Public Class NgEvidDnevnik
    Public Property id As Integer               ' npr 101   
    Public Property employeeID As Integer       ' npr 945
    Public Property sifra_placanja As String    ' npr 'PLC'
    Public Property datum As String             ' "2020-02-17T12:00:00"
    Public Property week_day As Integer          '  0 - pon ... 6-ned
    Public Property vrijeme_od As String        ' '08:00'
    Public Property vrijeme_do As String        ' '12:00'
    Public Property vrijeme_ukupno As String    ' '18:00'
    Public Property opis_rada As String         ' 'INO požari Australija - prilog'
    Public Property keyday As Boolean           ' true: osnovni datum mjeseca
    Public Property evd_unio As Integer
    Public Property locked As Boolean           '
    Public Property evd_podnio As Integer
    Public Property evd_odobrio As Integer
    Public Property evd_kontrolisao As Integer
    Public Property locked_ext As Boolean       ' 
End Class
