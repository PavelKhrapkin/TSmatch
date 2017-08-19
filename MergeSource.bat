ECHO --- Pavel Khrapkin 2017.08.19. Merge TSmatch Git Repo from Source ---
cd C:\Users\Pavel_Khrapkin\Source\Repos\TSmatchSAV\.git
git remote add TSmatch C:\Users\Pavel_Khrapkin\Desktop\TSmatch\.git
git fetch TSmatch
git merge --allow-unrelated-histories TSmatch/master # or whichever branch you want to merge
git remote remove TSmatch
set /p temp="Hit enter to continue"