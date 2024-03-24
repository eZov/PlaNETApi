Imports Microsoft.VisualBasic

Public Class ClsDropDownDataYN

    Public Sub New(_id As Integer, _text As String)
        Me.ID = _id
        Me.Text = _text
    End Sub

    Public Sub New()
    End Sub

    <WebBrowsable(True)>
    Public Property ID() As Integer
        Get
            Return m_ID
        End Get
        Set
            m_ID = Value
        End Set
    End Property
    Private m_ID As Integer

    <WebBrowsable(True)>
    Public Property Text() As String
        Get
            Return m_Text
        End Get
        Set
            m_Text = Value
        End Set
    End Property
    Private m_Text As String

    Public Function GetItems() As List(Of ClsDropDownDataYN)

        Dim data As New List(Of ClsDropDownDataYN)()
        data.Add(New ClsDropDownDataYN(0, "Ne"))
        data.Add(New ClsDropDownDataYN(1, "Da"))

        Return data
    End Function

End Class
