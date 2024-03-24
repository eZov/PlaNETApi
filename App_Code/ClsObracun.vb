Imports Microsoft.VisualBasic

Public Class ClsObracun

    Public Property Id As Integer
    Public Property IdVrsta As Integer

    Public Property Mjesec As Integer
    Public Property Godina As Integer

    Public Property SatiUMjesecu As Integer = 0
    Public Property SatiOsnovica As Integer = 0

    Public Property OrgJed As Integer

    Public Property VriBoda1 As Double = 0
    Public Property VriBoda2 As Double = 0

    Public Property VriTopli As Double = 0

    Public Property DatIspl As Date
    Public Property DatDopr As Date

    Public Property SumaPlac As Double = 0
    Public Property SumaDopr As Double = 0
    Public Property SumaObust As Double = 0
    Public Property SumaNet As Double = 0

    Public Property Started As Boolean = False
    Public Property Completed As Boolean = False

    Public Property DateFrom As Date
    Public Property DateTo As Date

    Public Property SatiDrzPraznik As Integer = 0

    Public Property ProsjPlaca As Double = 0

    Public Property DatObr As Date

    Public Property LastDone As Boolean = False

    Public Property PorOlaksice As Boolean = False

End Class
