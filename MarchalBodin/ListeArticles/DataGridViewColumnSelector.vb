'  *****************************************
'  ** DataGridViewColumnSelector ver 1.0  **
'  **                                     **
'  ** Author : Vincenzo Rossi             **
'  ** Country: Naples, Italy              **
'  ** Year   : 2008                       **
'  ** Mail   : redmaster@tiscali.it       **
'  **                                     **
'  ** Released under                      **
'  **   The Code Project Open License     **
'  **                                     **
'  **   Please do not remove this header, **
'  **   I will be grateful if you mention **
'  **   me in your credits. Thank you     **
'  **                                     **
'  *****************************************
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Windows.Forms
Imports System.Drawing

Namespace DGVColumnSelector
    ''' <summary>
    ''' Add column show/hide capability to a DataGridView. When user right-clicks 
    ''' the cell origin a popup, containing a list of checkbox and column names, is
    ''' shown. 
    ''' </summary>
    Class DataGridViewColumnSelector
        ' the DataGridView to which the DataGridViewColumnSelector is attached
        Private mDataGridView As DataGridView = Nothing
        ' the My.Settings.[mUserSettingName] name
        Private mUserSettingName As String
        ' a CheckedListBox containing the column header text and checkboxes
        Private mCheckedListBox As CheckedListBox
        ' a ToolStripDropDown object used to show the popup
        Private mPopup As ToolStripDropDown

        ''' <summary>
        ''' The max height of the popup
        ''' </summary>
        Public MaxHeight As Integer = 300
        ''' <summary>
        ''' The width of the popup
        ''' </summary>
        Public Width As Integer = 200

        ''' <summary>
        ''' Gets or sets the DataGridView to which the DataGridViewColumnSelector is attached
        ''' </summary>
        Public Property DataGridView() As DataGridView
            Get
                Return mDataGridView
            End Get
            Set
                ' If any, remove handler from current DataGridView 
                If Not (mDataGridView Is Nothing) Then
                    RemoveHandler mDataGridView.CellMouseClick, AddressOf mDataGridView_CellMouseClick
                End If
                ' Set the new DataGridView
                mDataGridView = Value
                ' Attach CellMouseClick handler to DataGridView
                If Not (mDataGridView Is Nothing) Then
                    AddHandler mDataGridView.CellMouseClick, AddressOf mDataGridView_CellMouseClick
                End If
            End Set
        End Property

        ' When user right-clicks the cell origin, it clears and fill the CheckedListBox with
        ' columns header text. Then it shows the popup. 
        ' In this way the CheckedListBox items are always refreshed to reflect changes occurred in 
        ' DataGridView columns (column additions or name changes and so on).
        Private Sub mDataGridView_CellMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs)
            If e.Button = MouseButtons.Right AndAlso e.RowIndex = -1 AndAlso e.ColumnIndex > -1 Then
                mCheckedListBox.Items.Clear()
                For Each c As DataGridViewColumn In mDataGridView.Columns
                    mCheckedListBox.Items.Add(c.HeaderText, My.Settings.Item(mUserSettingName).Contains(c.HeaderText))
                Next
                Dim PreferredHeight As Integer = (mCheckedListBox.Items.Count * 16) + 7
                mCheckedListBox.Height = If((PreferredHeight < MaxHeight), PreferredHeight, MaxHeight)
                mCheckedListBox.Width = Me.Width
                mPopup.Show(mDataGridView, mDataGridView.PointToClient(Cursor.Position))
            End If
        End Sub

        ' The constructor creates an instance of CheckedListBox and ToolStripDropDown.
        ' the CheckedListBox is hosted by ToolStripControlHost, which in turn is
        ' added to ToolStripDropDown.
        Public Sub New()
            mCheckedListBox = New CheckedListBox()
            mCheckedListBox.CheckOnClick = True
            AddHandler mCheckedListBox.ItemCheck, AddressOf mCheckedListBox_ItemCheck
            AddHandler mCheckedListBox.LostFocus, AddressOf mCheckedListBox_LostFocus

            Dim mControlHost As New ToolStripControlHost(mCheckedListBox)
            mControlHost.Padding = Padding.Empty
            mControlHost.Margin = Padding.Empty
            mControlHost.AutoSize = False

            mPopup = New ToolStripDropDown()
            mPopup.Padding = Padding.Empty
            mPopup.Items.Add(mControlHost)
        End Sub

        ' Save settings on leave
        Private Sub mCheckedListBox_LostFocus(sender As Object, e As EventArgs)
            My.Settings.Item(mUserSettingName).Clear()
            For Each c As DataGridViewColumn In mDataGridView.Columns
                If c.Visible Then
                    My.Settings.Item(mUserSettingName).Add(c.HeaderText)
                End If
            Next
            My.Settings.Save()
        End Sub

        Public Sub New(dgv As DataGridView, ByRef stg As String)
            Me.New()
            Me.DataGridView = dgv
            mUserSettingName = stg

            If My.Settings.Item(mUserSettingName) Is Nothing Then
                My.Settings.Item(mUserSettingName) = New Specialized.StringCollection
                For Each c As DataGridViewColumn In mDataGridView.Columns
                    If c.Visible Then
                        My.Settings.Item(mUserSettingName).Add(c.HeaderText)
                    End If
                Next
                My.Settings.Save()
            End If
        End Sub

        ' When user checks / unchecks a checkbox, the related column visibility is 
        ' switched.
        Private Sub mCheckedListBox_ItemCheck(sender As Object, e As ItemCheckEventArgs)
            mDataGridView.Columns(e.Index).Visible = (e.NewValue = CheckState.Checked)
        End Sub
    End Class
End Namespace
