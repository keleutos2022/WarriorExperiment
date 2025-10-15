#!/bin/bash

# Get directory name and git branch name
DIR_NAME=$(basename "$PWD" | tr '.' '_')
GIT_BRANCH=$(git branch --show-current 2>/dev/null)
if [ -n "$GIT_BRANCH" ]; then
    # Sanitize branch name for tmux session (replace special chars)
    BRANCH_NAME=$(echo "$GIT_BRANCH" | sed 's/[^a-zA-Z0-9_-]/_/g')
    SESSION_NAME="${DIR_NAME}_${BRANCH_NAME}"
else
    # Fallback to directory name only
    SESSION_NAME="$DIR_NAME"
fi

# Check if session already exists
tmux has-session -t $SESSION_NAME 2>/dev/null

if [ $? != 0 ]; then
    # Create new session with claude window
    tmux new-session -d -s $SESSION_NAME -n "claude"
    
    # Display tmux shortcuts in the first window
    tmux send-keys -t $SESSION_NAME:0 "clear && cat << 'EOF'
=== TMUX SHORTCUTS ===

Navigation:
  Ctrl+b n       - Next window
  Ctrl+b p       - Previous window
  Ctrl+b [0-9]   - Jump to window by number
  Ctrl+b w       - List windows
  Ctrl+b ,       - Rename current window

Panes:
  Ctrl+b %       - Split vertically
  Ctrl+b \"       - Split horizontally
  Ctrl+b ↑↓←→    - Move between panes
  Ctrl+b x       - Close current pane
  Ctrl+b z       - Toggle pane zoom

Sessions:
  Ctrl+b d       - Detach from session
  Ctrl+b s       - List sessions
  Ctrl+b $       - Rename session

Copy Mode:
  Ctrl+b [       - Enter copy mode
  q              - Exit copy mode
  Space          - Start selection
  Enter          - Copy selection

Other:
  Ctrl+b ?       - Show all key bindings
  Ctrl+b :       - Command prompt

=== ASTRONVIM SHORTCUTS ===

Navigation:
  <Space> e      - Toggle file explorer (Neo-tree)
  <Space> ff     - Find files (Telescope)
  <Space> fw     - Find word/grep (Telescope)
  <Space> fb     - Find buffers
  <Space> /      - Find in current buffer
  Ctrl+h/j/k/l   - Navigate between splits

Buffers/Tabs:
  <Shift> h      - Previous buffer
  <Shift> l      - Next buffer
  <Space> c      - Close buffer
  <Space> C      - Force close buffer
  <Space> b      - Buffer picker

Code Navigation:
  gd             - Go to definition
  gr             - Go to references
  K              - Hover documentation
  <Space> lf     - Format code
  <Space> la     - Code actions
  <Space> lr     - Rename symbol
  <Space> ld     - Show diagnostics

Git:
  <Space> gg     - Open LazyGit
  <Space> gj     - Next git hunk
  <Space> gk     - Previous git hunk
  <Space> gp     - Preview git hunk
  <Space> gs     - Stage git hunk

Terminal:
  <Space> tf     - Float terminal
  <Space> th     - Horizontal terminal
  <Space> tv     - Vertical terminal
  Ctrl+\\        - Toggle terminal

Other:
  <Space> h      - Show help/cheatsheet
  :w             - Save file
  :q             - Quit
  :wq            - Save and quit
  u              - Undo
  Ctrl+r         - Redo

Current Session: $SESSION_NAME
EOF" C-m
    
    # Create additional windows
    tmux new-window -t $SESSION_NAME:0 -n "claude"
    tmux new-window -t $SESSION_NAME:1 -n "claude"
    tmux new-window -t $SESSION_NAME:2 -n "claude"
    tmux new-window -t $SESSION_NAME:3 -n "nvim"
    tmux new-window -t $SESSION_NAME:4 -n "shell1"
    tmux new-window -t $SESSION_NAME:5 -n "shell2"
    tmux new-window -t $SESSION_NAME:6 -n "shell3"
    tmux new-window -t $SESSION_NAME:9 -n "run"
    
    # Set up default commands for each window
    tmux send-keys -t $SESSION_NAME:0 "claude" C-m
    tmux send-keys -t $SESSION_NAME:1 "claude" C-m
    tmux send-keys -t $SESSION_NAME:2 "claude" C-m
    tmux send-keys -t $SESSION_NAME:3 "nvim" C-m
    tmux send-keys -t $SESSION_NAME:9 "dotnet run --project WarriorExperiment.App/WarriorExperiment.App.csproj --profile http" C-m
        
    # Select first claude window
    tmux select-window -t $SESSION_NAME:0
fi

# Attach to session
tmux attach-session -t $SESSION_NAME
