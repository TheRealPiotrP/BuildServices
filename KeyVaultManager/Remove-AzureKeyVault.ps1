#.ExternalHelp KeyVaultManager.psm1-help.xml

Function Remove-AzureKeyVault{
    Param(
        [Parameter(Mandatory=$true,Position=0,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the name of the Azure key vault name to remove")]
        [ValidateNotNullOrEmpty()]
        [String] $VaultName
    ,
        [Parameter(Mandatory=$false,Position=1,ValueFromPipelineByPropertyName=$true,HelpMessage="Enter the subscription name you want to use for this process or your account's default subscription will be used")] 
        [String] $SubscriptionName
    ,
        [switch] $Force
    )
    
    Setup-KeyVaultEnv $SubscriptionName    

    # Swith to Resource Manager
    Switch-ToResourceManager
    
    $vault = Get-AzureResource | where {$_.Name -eq $VaultName -and $_.ResourceType -eq $KeyVaultResourceType}

    if($vault)
    {
        $vaultId = @{
            "ResourceType" = $KeyVaultResourceType;
            "ApiVersion" = $KeyVaultApiVersion;
            "ResourceGroupName" = $vault.ResourceGroupName;
            "Name" = $vaultName;
        }
        if($Force)
        {
            Remove-AzureResource @vaultId -Force
        }
        else
        {
            Remove-AzureResource @vaultId
        }
        Write-Host 'The vault' $VaultName 'is removed!' -ForegroundColor Green
    }
    else
    {
        Write-Host 'The vault' $VaultName 'is not found!' -ForegroundColor Yellow
    }
}