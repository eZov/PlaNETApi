Imports Microsoft.VisualBasic

Public Class ClsPutniNalog
    Public putninalog_id As Integer
    Public employee_id As Integer

    Public radno_mjesto As String
    Public mjesto_putovanja As String
    Public razlog_putovanja As String
    Public dat_poc_putovanja As String
    Public trajanje_putovanja As String
    Public prev_auto As Boolean
    Public prev_vlauto As Boolean
    Public prev_avion As Boolean
    Public prev_autobus As Boolean
    Public prevoz_voz As Boolean

    Public odobrena_akontacija As Boolean

    Public iznos_akontacije As String
    Public status_naloga As Integer

    Public ime_prezime As String

    Public troskovi_putovanja_knjizenje As String
End Class
