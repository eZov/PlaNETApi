Imports Microsoft.VisualBasic

Public Class NgECertificate
    Public Property id As Integer              ' id reda u tabeli
    Public Property employeeID As Integer      ' dobija preko POST - EmployeeID POTPISNIKA
    Public Property subjectCN As String
    Public Property issuerCN As String
    Public Property bitStrength As Integer
    Public Property hType As HashType
    Public Property [cType] As ContainerType
    Public Property validFrom As DateTime
    Public Property validTo As DateTime
    Public Property validYears As Integer
    Public Property password As String

    Public Property X509Name_C As String = "BA"                                 ' country code
    Public Property X509Name_O As String                                        ' organization
    Public Property X509Name_L As String                                        ' locality name
    Public Property X509Name_ST As String                                       ' state, or province name 
    Public Property X509Name_E As String                                        ' email address - organization

    Public Property X509Name_EmailAddress As String                             ' email address
    Public Property X509Name_SN As String                                       ' device serial number name 

    Public Property signPos As Integer = 1                                      ' definiše poziciju potpisa na pdf dokumentu: 1, 2, ...
    Public Property ecertStatus As String                                       ' statusi:  "no_signature", "no_image", "ready for certificate"
    Public Property ecertOdobrenje As String                                       ' statusi:  "no_signature", "no_image", "ready for certificate"
    Public Property ImgExist As Boolean = False                                 ' slika potpisa postoji u db tabeli
    Public Property PfxExist As Boolean = False                                 ' pfx certifikat postoji u db tabeli

    'url dokumentacije: https://people.eecs.berkeley.edu/~jonah/bc/org/bouncycastle/asn1/x509/X509Name.html

    ' STATUSI:
    '   no_signature            - nema reda u db tabeli, admin mora pripremiti prazan red
    '   no_image                - postoji red u db tabeli, ali nije upisana slika potpisa
    '   ready for certificate   - postoji redu tabeli, upisana slika potpisa, može se generisati pfx certifikat
    '   certificate valid       - postoji red u tabeli, slika upisana, pfx certifikat generisan, potpisa aktivan ...
    '   certificate invalid       - postoji red u tabeli, slika upisana, pfx certifikat generisan, potpisa neaktivan ...

End Class
