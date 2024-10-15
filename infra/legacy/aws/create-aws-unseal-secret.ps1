# Step 1: Creating a KMS Key
$keyDescription = "np-sync-keyvault-unseal" # replace with your desired description

# Create KMS key
$kmsKey = aws kms create-key --description $keyDescription | ConvertFrom-Json

# Step 2: Creating an IAM User and Granting Access to the KMS Key
$userName = "np-sync-keyvault-sa" # replace with your desired username
$policyName = "np-sync-keyvault-unseal-access" # replace with your desired policy name

# Create IAM user with programmatic access
aws iam create-user --user-name $userName | ConvertFrom-Json
$creds = aws iam create-access-key --user-name $userName | ConvertFrom-Json

# Create a policy that grants access to the KMS key
$policyDocument = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": "kms:*",
      "Resource": "$($kmsKey.KeyMetadata.Arn)"
    }
  ]
}
"@
$policyFile = "policy.json"
$policyDocument | Out-File -Path $policyFile
$newPolicy = aws iam create-policy --policy-name $policyName --policy-document file://$policyFile | ConvertFrom-Json
Remove-Item $policyFile

# Attach the policy to the user
aws iam attach-user-policy --user-name $userName --policy-arn $newPolicy.Policy.Arn

# Create a temporary AWS credentials file
$awsCredsFile = "./secrets/aws-unseal"
@"
[default]
aws_access_key_id = $($creds.AccessKey.AccessKeyId)
aws_secret_access_key = $($creds.AccessKey.SecretAccessKey)
"@ | Out-File -Path $awsCredsFile

# Create a Kubernetes secret using kubectl from the AWS credentials file
kubectl create secret generic np-sync-aws-unseal --from-file=credentials=$awsCredsFile

# Cleanup and remove the temporary AWS credentials file
Remove-Item $awsCredsFile
