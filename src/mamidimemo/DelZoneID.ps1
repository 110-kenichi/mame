Write-Output "Removing Zone.Identifier..."
Get-ChildItem -Recurse -File | ?{ $_ | Get-Item -Stream Zone.Identifier -ErrorAction Ignore; } | Remove-Item -Stream Zone.Identifier;
Write-Output "Done."
pause
