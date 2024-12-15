# Install necessary modules
if (-not (Get-Module -ListAvailable -Name powershell-yaml)) {
    Install-Module -Name powershell-yaml -Force
}

Import-Module powershell-yaml

# Path to your YAML file
$yamlFilePath = "configuration.yaml"  # Replace with the actual path to your YAML file

# Read and parse the YAML file
$yamlContent = (Get-Content -Raw -Path $yamlFilePath) | ConvertFrom-Yaml

# Extract IAM information
$iamUserName = $yamlContent.iam.username
$iamPolicyName = $yamlContent.iam.policy

# Extract secrets information
$prefix = $yamlContent.secrets.prefix
$region = $yamlContent.secrets.region
# $secretData = $yamlContent.secrets | Where-Object { $_.Key -notin @('prefix', 'region') }
$secretData = $yamlContent.secrets.GetEnumerator() | Where-Object { $_.Key -notin @('prefix', 'region') }

# Write-Host $secretData

# exit 0

# AWS Account ID
$accountId = (aws sts get-caller-identity | ConvertFrom-Json).Account

# Create the secrets in AWS Secrets Manager
foreach ($secret in $secretData.GetEnumerator()) {
    $key = $secret.Key
    $value = $secret.Value
    $secretName = "$prefix/$key"
    
    Write-Host "Processing secret: $secretName"    

    try {
        # Check if the secret already exists
        $existingSecret = aws secretsmanager describe-secret --secret-id $secretName --region $region --output json 2>$null | ConvertFrom-Json
        if ($null -eq $existingSecret) {
            # Create the secret
            aws secretsmanager create-secret --name $secretName --secret-string "$value" --region $region            
            Write-Host "Secret $secretName created successfully."
        } else {
            # Update the secret
            Write-Host "Secret $secretName already exists. Updating value."
            aws secretsmanager put-secret-value --secret-id $secretName --secret-string "$value" --region $region
            Write-Host "Secret $secretName updated successfully."
        }
    } catch {
        Write-Host "Error processing secret ${secretName}: $_"
    }    
}

# Create the IAM user
Write-Host "`nCreating IAM user: $iamUserName"
try {
    $existingUser = aws iam get-user --user-name $iamUserName --output json 2>$null | ConvertFrom-Json
    if ($null -eq $existingUser) {
        aws iam create-user --user-name $iamUserName
        Write-Host "IAM user $iamUserName created successfully."
    } else {
        Write-Host "IAM user $iamUserName already exists."
    }
} catch {
    Write-Host "Error creating IAM user ${iamUserName}: $_"
}

# Create IAM policy
Write-Host "`nCreating IAM policy: $iamPolicyName"
$secretArnPattern = "arn:aws:secretsmanager:${region}:${accountId}:secret:${prefix}/*-*"
$policyDocument = @{
    Version = "2012-10-17"
    Statement = @(
        @{
            Sid      = "AllowAccessToSecrets"
            Effect   = "Allow"
            Action   = @(
                "secretsmanager:GetSecretValue",
                "secretsmanager:PutSecretValue",
                "secretsmanager:CreateSecret",
                "secretsmanager:DeleteSecret",
                "secretsmanager:UpdateSecret"
            )
            Resource = $secretArnPattern
        }
    )
} | ConvertTo-Json -Depth 5

$policyFile = "policy.json"
$policyDocument | Out-File -Encoding ascii -FilePath $policyFile

try {
    $existingPolicy = aws iam get-policy --policy-arn "arn:aws:iam::$accountId:policy/$iamPolicyName" --output json 2>$null | ConvertFrom-Json
    if ($null -eq $existingPolicy) {
        aws iam create-policy --policy-name $iamPolicyName --policy-document file://$policyFile
        Write-Host "IAM policy $iamPolicyName created successfully."
    } else {
        Write-Host "IAM policy $iamPolicyName already exists."
    }
} catch {
    Write-Host "Error creating IAM policy ${iamPolicyName}: $_"
}

# Cleanup policy file
Remove-Item $policyFile

# Attach the policy to the user
Write-Host "`nAttaching policy to user"
try {
    aws iam attach-user-policy --user-name $iamUserName --policy-arn "arn:aws:iam::${accountId}:policy/${iamPolicyName}"
    Write-Host "Policy $iamPolicyName attached to user $iamUserName successfully."
} catch {
    Write-Host "Error attaching policy: $_"
}

# Prompt user to create access keys
$createKeys = Read-Host "Do you want to create access keys for the user $iamUserName? (default: No) [Yes/No]"
if ($createKeys -eq "Yes") {
    Write-Host "`nCreating access keys for user $iamUserName"
    try {
        $accessKey = aws iam create-access-key --user-name $iamUserName --output json | ConvertFrom-Json
        Write-Host "Access key created successfully."

        # Display command to apply access keys to Kubernetes cluster
        $applyToK8s = Read-Host "Do you want to apply these access keys to a Kubernetes cluster? (default: No) [Yes/No]"
        if ($applyToK8s -eq "Yes") {
            $namespace = Read-Host "Enter the Kubernetes namespace"
            $kubectlCommand = "kubectl create secret generic np-worker-aws-secret --from-literal='aws_access_key_id=$($accessKey.AccessKey.AccessKeyId)' --from-literal='aws_secret_access_key=$($accessKey.AccessKey.SecretAccessKey)' -n $namespace"
            $kubectlCommandForDisplay = "kubectl create secret generic np-worker-aws-secret --from-literal='aws_access_key_id=$($accessKey.AccessKey.AccessKeyId)' --from-literal='aws_secret_access_key=REDACTED' -n $namespace"
            Write-Host "Command to apply the secret to Kubernetes:"
            Write-Host $kubectlCommandForDisplay
            $confirm = Read-Host "Do you want to execute this command? (default: No) [Yes/No]"
            if ($confirm -eq "Yes") {
                Invoke-Expression $kubectlCommand
                Write-Host "Secret applied to Kubernetes cluster successfully."
            } else {
                Write-Host "Command not executed."
            }
        }
    } catch {
        Write-Host "Error creating access keys: $_"
    }
}

Write-Host "`nAWS setup completed successfully."
