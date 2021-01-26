$region="us-east-1"

$deploymentbucketname="<YOUR-BUCKET-NAME>"

# Create deployment bucket if it doesn't exist
if ($region -eq "us-east-1") {
    aws s3api create-bucket --bucket $deploymentbucketname --region $region
} else {
    aws s3api create-bucket --bucket $deploymentbucketname --region $region --create-bucket-configuration LocationConstraint=$region
}

# Build, package and deploy the backend
sam build
sam package --s3-bucket $deploymentbucketname --output-template-file gameservice.yaml
sam deploy --template-file gameservice.yaml --region $region --capabilities CAPABILITY_IAM --stack-name gameservice-backend