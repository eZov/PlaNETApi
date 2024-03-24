Imports Microsoft.VisualBasic

Public Class NgESignature
    Public Property pPutNalID As Integer                ' dobija preko POST - ID putnog naloga
    Public Property pEmployeeID As Integer              ' dobija preko POST - EmployeeID POTPISNIKA putnog naloga
    Public Property pPutNalESignature As String         ' dobija preko POST - Potpis putnog naloga
    Public Property ePublicKey As String = ""             ' uzima iz baze
    Public Property ePublicKeyId As String = -1           ' uzima iz baze
    Public Property ePublicKeyStatus As String = ""       ' no_signature / create_keys / activated / deactivated / no_signature
End Class
