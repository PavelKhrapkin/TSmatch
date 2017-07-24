cd C:\Users\khrapkin\Documents\Pavel\@TSmatch\Releases\TSmatchSAV
git remote add TSmatch C:\Users\khrapkin\Source\Repos\TSmatchSAV
git fetch TSmatch
git merge --allow-unrelated-histories TSmatchSAV/master # or whichever branch you want to merge
git remote remove TSmatch