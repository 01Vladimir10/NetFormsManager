# Design Guidelines

## Color Palette
- **Primary**: Indigo 500 (`#6366f1`) - Use for primary buttons, active states, and key accents.
- **Secondary**: Pink 500 (`#ec4899`) - Use for secondary actions or highlights.
- **Background**: Slate 50 (`#f8fafc`) - Main background color.
- **Surface**: White (`#ffffff`) - Card backgrounds, modal backgrounds.
- **Text**: Slate 900 (`#0f172a`) - Primary text color.
- **Error**: Red 500 (`#ef4444`) - Error states.

## Typography
- **Font Family**: Inter (Google Fonts).
- **Headings**: Bold, Slate 900.
- **Body**: Regular, Slate 900.

## Components

### Buttons
- **Primary**: Indigo 500 background, White text. Rounded corners.
- **Secondary**: Pink 500 background, White text. Rounded corners.
- **Ghost**: Transparent background, Indigo 500 text.

### Cards
- White background.
- Subtle shadow (`shadow-sm` or `shadow-md`).
- Rounded corners (`rounded-lg`).
- Padding (`p-6`).

### Tables
- Use for lists of items (Forms, Subscribers).
- Header: Slate 50 background, Slate 900 text, bold.
- Rows: White background, border-b Slate 200.
- Hover: Slate 50.

### Inputs
- Floating labels (Material style).
- Minimalistic border.
- Focus: Primary color border.

## Layout
- **Sidebar/Navigation**: Fixed on the left or top.
- **Main Content**: Centered or fluid with max-width.
- **Spacing**: Use Tailwind's spacing scale (4, 8, 16, 24, 32px).
