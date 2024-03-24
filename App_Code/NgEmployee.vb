Imports Microsoft.VisualBasic

Public Class NgEmployee
    Public Property EmployeeID As Integer
    Public Property EmployeeNumber As String
    Public Property Title As String             ' Definiše status: S-saradnik, P-povremeni saradnik, K-korisnik
    Public Property JMB As String               ' JMB
    Public Property FirstName As String         ' Ime
    Public Property MiddleName As String        'HACK: iskoristit za rođeno/djevojačko prezime
    Public Property LastName As String          ' Prezime
    Public Property BrSocOsig As String
    Public Property Opc_Stanovanja As String    ' Šifra općine ŠIFARNIK OPĆINE
    Public Property Djeca As Integer            ' maloljetna djeca
    Public Property RadnoMjesto As Integer      ' radno mjesto po sistematizaciji ŠIFARNIK SISTEMATIZACIJE
    Public Property PostalCode As String        ' pošta kojoj pripada
    Public Property DepartmentID As Integer     ' id org.jed. za obračun ŠIFARNIK ORGANIZACIJE
    Public Property OfficeLocation As String    ' adresa radne lokacije
    Public Property Dat_Rodjenja As String      ' datum rođenja
    Public Property Dat_Zaposl As String        ' datum zaposlenja
    Public Property Dat_Odlaska As String       ' datum odlaska
    Public Property Aktivan As Boolean          ' aktivan/neaktivan UPOSLENIK
    Public Property Razlog_Odlaska As String    ' razlog prekida odnosa
    Public Property DepartmentUP As Integer     ' org.jed. uposlenika / saradnika; -1 za korisnike sistema
    Public Property Pol As String
    Public Property ImeRoditelja As String
    Public Property Broj_LK As String
    Public Property Mjesto_LK As String
    Public Property Opcina_LK As String
    Public Property Drzavljanstvo As String
    Public Property Narod As String
    Public Property BracnoStanje As String
    Public Property emp_hm_telephone As String
    Public Property emp_hm_mobile As String
    Public Property emp_oth_email As String
    Public Property emp_work_extphone As String
    Public Property emp_work_telephone As String
    Public Property emp_work_mobile As String
    Public Property emp_work_email As String
    Public Property emp_work_default_email As Integer
    Public Property emp_username As String
    Public Property work_station As Integer
    Public Property emp_email_exclude As String
End Class
