set -euo pipefail
shopt -s nullglob

REPO="66six11/Attribute-SaveFramework-For-Unity"
WIKI_URL="https://x-access-token:${GITHUB_TOKEN}@github.com/${REPO}.wiki.git"

echo "Configuring git user..."
git config --global user.email "github-actions[bot]@users.noreply.github.com"
git config --global user.name "github-actions[bot]"
# Optional: avoid safe.directory warnings
git config --global --add safe.directory '*'

echo "Cloning wiki repo..."
rm -rf wiki-repo
git clone "${WIKI_URL}" wiki-repo

echo "Copying generated wiki files..."
# If there are no files, this won't fail due to nullglob
cp -f wiki/*.md wiki-repo/ || true

cd wiki-repo

echo "Staging changes..."
git add -A

# If nothing staged, exit gracefully
if git diff --cached --quiet; then
  echo "No changes to commit to Wiki."
  exit 0
fi

echo "Committing..."
git commit -m "Auto-update wiki from XML documentation [skip ci]"

# Push to the current branch (Wiki typically uses 'master')
echo "Pushing..."
git push origin HEAD

echo "Wiki updated successfully!"
