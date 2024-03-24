Imports Microsoft.VisualBasic

<Serializable()> Public Class ClsEmployee


    Public Property EmployeeId As Integer = -1
    Public Property FirstName As String = ""
    Public Property LastName As String = ""

    Public Property JMB As String = ""

    Public Property PersBroj As String = ""
    Public Property SocOsBroj As String = ""

    Public Property BornName As String = ""

    Public Property Pol As String = ""
    Public Property PolM As Boolean
    Public Property PolF As Boolean

    Public Property OrgJedObr As Integer = -1
    Public Property OrgJedUpo As Integer = -1

    Public Property OrgJedObrXfr As String = ""
    Public Property OrgJedUpoXfr As String = ""

    Public Property RadMjes As Integer = -1
    Public Property RadMjesXfr As String = ""

    Public Property StrSpr As String = ""
    Public Property StrSpXfr As String = ""

    Public Property OpcStanovanja As String = ""
    Public Property Adresa As String = ""
    Public Property AdresaBr As String = ""

    Public Property NepunoRadVr As Boolean
    Public Property NepunoRadVrProc As Integer = -1

    Public Property DatZaposlenja As Date
    Public Property DateOdlaska As Date

    Public Property stazPret As Double
    Public Property stazPretPoslodavac As Double
    Public Property stazKont As Double
    Public Property stazUkup As Double

    Public Property procMinRad As Double

    Public Property Banka As String
    Public Property TekRacun As String

    Public Property BrutoPlata As Double
    Public Property NetoPlata As Double

    Public Property PorKoef12 As Double
    Public Property PorKoef13 As Double
    Public Property PorKoef14 As Double
    Public Property PorKoef15 As Double
    Public Property PorKoef16 As Double
    Public Property PorKoef17 As Double

    Public Property PorodIznos As Double
    Public Property PorodRjes As String
    Public Property PorodOd As Date
    Public Property PorodDo As Date

    Public Property Reserve_1 As String
    Public Property Reserve_2 As String
End Class
