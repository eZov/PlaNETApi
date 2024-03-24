Imports System.Data
Imports Microsoft.VisualBasic

Public Class ClsEmployeeGodOdm

    Public Sub New()
        Kriteriji = New DataTable
    End Sub

    Public Property EmployeeId As Integer = -1
    Public Property Godina As Integer = 0

    Public Property Kriteriji As DataTable = Nothing

    Public Property DaniGO As Integer = 0

    Public Property Staz As String = ""

    Public Property RjesStatus As Integer = 0
    Public Property EvdLocked As Integer = 0

    Public Property plan1StartDate As Date = Nothing
    Public Property plan1EndDate As Date = Nothing
    Public Property plan1RadDate As Date = Nothing
    Public Property plan1Dani As Integer = 0

    Public Property plan2StartDate As Date = Nothing
    Public Property plan2EndDate As Date = Nothing
    Public Property plan2RadDate As Date = Nothing
    Public Property plan2Dani As Integer = 0

    Public Property DaniSGO As Integer = 0

    Public Property PreneseniSGO As Integer = 0
    Public Property PreneseniNGO As Integer = 0

    Public Property AnulSGO As Integer = 0
    Public Property AnulNGO As Integer = 0

    Public Property docMD5 As String = ""

    Public Property rjPotpisnik As String = ""
    Public Property rjProt1 As String = ""
    Public Property rjProt2 As String = ""
    Public Property rjDatRjes As Date

End Class
