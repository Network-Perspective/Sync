#!/bin/bash

openssl genrsa -out secrets/key.pem 4096
openssl rsa -in secrets/key.pem -outform PEM -pubout -out secrets/public.pem
