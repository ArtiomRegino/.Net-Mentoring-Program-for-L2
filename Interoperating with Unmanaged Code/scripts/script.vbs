Dim mObj, blnValid
' Create
set mObj = CreateObject("PowerStateManagemet.PowerStateManagemet")
blnValid = mObj.SetSuspendState(true,true)
