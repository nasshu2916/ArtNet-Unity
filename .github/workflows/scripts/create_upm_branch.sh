#!/bin/bash -e

# This script create a branch.
git checkout master

git config user.name "github-actions[bot]"
git config user.email "github-actions[bot]@users.noreply.github.com"

git subtree split -P "$ROOT_DIR_PATH" -b $UPM_BRANCH
git checkout $UPM_BRANCH

for file in $ROOT_FILES; do
    git checkout master $file &> /dev/null || echo $file is not found
    if [ -f $file ]; then
        cp package.json.meta $file.meta
        UUID=$(cat uuidgen | tr -d '-')
        sed -i -e "s/guid:.*$/guid: $UUID/" $file.meta
        git add $file.meta
    fi
done

git mv Samples Samples~ &> /dev/null || echo Samples is not found
git rm Samples.meta
git commit -m "release $UPM_BRANCH"
git push -f origin $UPM_BRANCH
