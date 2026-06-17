# Complete Guide to Using GitHub

A practical, beginner-to-advanced guide to using Git and GitHub. Work through it
top to bottom, or jump to the section you need.

## Table of Contents

1. [What GitHub Actually Is](#1-what-github-actually-is)
2. [One-Time Setup](#2-one-time-setup)
3. [Core Mental Model](#3-core-mental-model)
4. [Starting a Repository](#4-starting-a-repository)
5. [The Daily Workflow](#5-the-daily-workflow)
6. [Branching](#6-branching)
7. [Pull Requests](#7-pull-requests)
8. [Collaborating on Others' Projects (Fork & PR)](#8-collaborating-on-others-projects-fork--pr)
9. [Handling Merge Conflicts](#9-handling-merge-conflicts)
10. [Essential Files Every Repo Should Have](#10-essential-files-every-repo-should-have)
11. [GitHub Features Beyond Code](#11-github-features-beyond-code)
12. [Commands Worth Knowing for Recovery](#12-commands-worth-knowing-for-recovery)
13. [A Suggested Learning Path](#13-a-suggested-learning-path)
14. [Best Official Resources](#14-best-official-resources)

---

## 1. What GitHub Actually Is

- **Git** is a *version control system* — a tool that tracks changes to files
  over time, lets you undo mistakes, and lets many people work on the same code
  without overwriting each other.
- **GitHub** is a *website/cloud service* that hosts Git repositories online. It
  adds collaboration features on top of Git: issues, pull requests, reviews,
  CI/CD, project boards, etc.

Think of it this way: Git is the engine on your computer; GitHub is the shared
garage where everyone parks and works on the cars together.

## 2. One-Time Setup

```bash
# Install git (varies by OS)
#   macOS:    brew install git
#   Ubuntu:   sudo apt install git
#   Windows:  download from https://git-scm.com

# Tell git who you are (shows up in your commits)
git config --global user.name "Your Name"
git config --global user.email "you@example.com"

# Recommended defaults
git config --global init.defaultBranch main
git config --global pull.rebase false
```

Then create a free account at [github.com](https://github.com).

**Authentication** (so your computer can talk to GitHub):

- Easiest: install the [GitHub CLI](https://cli.github.com) and run `gh auth login`.
- Or create an **SSH key**:

```bash
ssh-keygen -t ed25519 -C "you@example.com"
cat ~/.ssh/id_ed25519.pub   # copy this, paste into GitHub > Settings > SSH keys
```

## 3. Core Mental Model

A few key concepts. Everything else builds on these:

| Term | Meaning |
|------|---------|
| **Repository (repo)** | A project folder tracked by Git |
| **Commit** | A saved snapshot of your changes with a message |
| **Branch** | An independent line of work (e.g. `main`, `feature-x`) |
| **Remote** | The online copy (usually GitHub), nicknamed `origin` |
| **Clone** | Download a repo to your computer |
| **Push** | Upload your commits to GitHub |
| **Pull** | Download others' commits from GitHub |
| **Pull Request (PR)** | A proposal to merge your branch into another |

The typical change flow:
**edit files → stage → commit → push → open pull request → review → merge.**

## 4. Starting a Repository

**Option A — Start on GitHub:** Click **New repository**, then clone it:

```bash
git clone https://github.com/yourname/your-repo.git
cd your-repo
```

**Option B — Start locally and connect it:**

```bash
mkdir my-project && cd my-project
git init
echo "# My Project" > README.md
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/yourname/my-project.git
git push -u origin main
```

## 5. The Daily Workflow

The 90% you'll use constantly:

```bash
git status                 # what's changed?
git add file.txt           # stage a specific file
git add .                  # stage everything
git commit -m "Describe what you did"
git push                   # send commits to GitHub

git pull                   # get latest changes from GitHub
```

**Good commit messages** matter. Use the imperative mood: "Add login
validation", not "added stuff".

## 6. Branching

How real work gets done. Never work directly on `main` in a team. Make a branch:

```bash
git checkout -b feature/login   # create + switch to a new branch
# ...make changes, commit them...
git push -u origin feature/login
```

Then on GitHub you'll see a prompt to **open a Pull Request**.

Useful branch commands:

```bash
git branch                  # list branches
git checkout main           # switch to main
git switch feature/login    # newer alternative to checkout
git merge feature/login     # merge a branch into your current one
git branch -d feature/login # delete a merged branch
```

## 7. Pull Requests

The heart of GitHub collaboration. A PR is where you say "here are my changes,
please review and merge them." On GitHub:

1. Push your branch.
2. Click **Compare & pull request**.
3. Write a clear title and description (what changed and why).
4. Reviewers comment, request changes, or approve.
5. You push more commits to address feedback (they appear in the PR automatically).
6. Once approved, click **Merge**.

This is how you contribute to other people's projects too: **fork** their repo
(your own copy), push changes to your fork, then open a PR back to the original.

## 8. Collaborating on Others' Projects (Fork & PR)

```bash
# 1. Click "Fork" on GitHub to copy the repo to your account
# 2. Clone YOUR fork
git clone https://github.com/yourname/their-repo.git
cd their-repo

# 3. Add the original as "upstream" so you can stay in sync
git remote add upstream https://github.com/original-owner/their-repo.git

# 4. Make a branch, commit, push to your fork, open a PR
git checkout -b fix/typo
git push -u origin fix/typo

# 5. Keep your fork updated later
git fetch upstream
git merge upstream/main
```

## 9. Handling Merge Conflicts

When two people change the same lines, Git asks you to decide. You'll see
markers like:

```text
<<<<<<< HEAD
your version
=======
their version
>>>>>>> branch-name
```

Edit the file to keep what you want, remove the markers, then:

```bash
git add the-file.txt
git commit
```

## 10. Essential Files Every Repo Should Have

- **`README.md`** — explains what the project is and how to use it (rendered on
  the repo homepage).
- **`.gitignore`** — lists files Git should ignore (e.g. `node_modules/`,
  `.env`, build output). Templates: [github.com/github/gitignore](https://github.com/github/gitignore).
- **`LICENSE`** — tells others how they're allowed to use your code.

## 11. GitHub Features Beyond Code

- **Issues** — track bugs, tasks, and feature requests. Reference them in
  commits with `#12`.
- **Pull Request reviews** — line-by-line comments and approvals.
- **Actions** — automated CI/CD: run tests, build, deploy on every push. Defined
  in `.github/workflows/*.yml`.
- **Projects** — Kanban-style boards to organize work.
- **Releases & Tags** — mark official versions (`v1.0.0`).
- **GitHub Pages** — free website hosting straight from a repo.
- **Wiki & Discussions** — documentation and community Q&A.

## 12. Commands Worth Knowing for Recovery

```bash
git log --oneline          # view commit history compactly
git diff                   # see unstaged changes
git restore file.txt       # discard changes to a file
git reset --soft HEAD~1    # undo last commit, keep changes staged
git revert <commit>        # make a new commit that undoes an old one
git stash                  # temporarily shelve changes
git stash pop              # bring them back
```

## 13. A Suggested Learning Path

1. Make a personal repo and practice the daily workflow (sections 5–6) until
   it's muscle memory.
2. Open a PR on your own repo from a branch (section 7).
3. Fork a friendly open-source project and submit a small fix like a typo
   (section 8).
4. Add a GitHub Actions workflow that runs tests automatically (section 11).
5. Learn rebasing and interactive history editing once the basics feel
   comfortable.

## 14. Best Official Resources

- [GitHub Skills](https://skills.github.com) — free interactive courses
  (highly recommended for hands-on practice).
- [GitHub Docs](https://docs.github.com) — comprehensive reference.
- [Pro Git book](https://git-scm.com/book) — free, the definitive Git deep-dive.
- [Oh My Git!](https://ohmygit.org) — a game for learning Git visually.
