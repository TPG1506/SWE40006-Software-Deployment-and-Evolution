<?php
declare(strict_types=1);

// Temp folder = writable locally and on Azure (cleared on restart).
$dataDir  = sys_get_temp_dir() . '/guestbook';
$dataFile = $dataDir . '/notes.json';

if (!is_dir($dataDir)) {
    mkdir($dataDir, 0755, true);
}

function e(string $text): string
{
    return htmlspecialchars($text, ENT_QUOTES, 'UTF-8');
}

function loadNotes(string $file): array
{
    if (!is_file($file)) {
        return [];
    }
    $decoded = json_decode((string) file_get_contents($file), true);
    return is_array($decoded) ? $decoded : [];
}

$errors = [];

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $name    = trim((string) ($_POST['name'] ?? ''));
    $message = trim((string) ($_POST['message'] ?? ''));

    if ($name === '' || strlen($name) > 50) {
        $errors[] = 'Name is required (max 50 characters).';
    }
    if ($message === '' || strlen($message) > 300) {
        $errors[] = 'Message is required (max 300 characters).';
    }

    if (!$errors) {
        $notes = loadNotes($dataFile);
        array_unshift($notes, [
            'name'    => $name,
            'message' => $message,
            'time'    => date('Y-m-d H:i'),
        ]);
        file_put_contents($dataFile, json_encode(array_slice($notes, 0, 50)), LOCK_EX);
        header('Location: index.php');   // redirect so a refresh doesn't re-post
        exit;
    }
}

$notes = loadNotes($dataFile);
?>
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1'>
  <title>PHP Guestbook</title>
  <style>
    body { font-family: system-ui, sans-serif; max-width: 640px; margin: 2rem auto; padding: 0 1rem; }
    input, textarea { width: 100%; padding: .5rem; margin: .25rem 0 1rem; box-sizing: border-box; }
    button { padding: .5rem 1.25rem; }
    .error { color: #b00020; }
    .note { border-bottom: 1px solid #ddd; padding: .75rem 0; }
    small { color: #666; }
  </style>
</head>
<body>
  <h1>PHP Guestbook</h1>
  <p><small>Running on PHP <?= e(PHP_VERSION) ?></small></p>

  <?php foreach ($errors as $err): ?>
    <p class='error'><?= e($err) ?></p>
  <?php endforeach; ?>

  <form method='post'>
    <label>Name <input name='name' maxlength='50' required></label>
    <label>Message <textarea name='message' rows='3' maxlength='300' required></textarea></label>
    <button type='submit'>Post</button>
  </form>

  <h2>Messages</h2>
  <?php if (!$notes): ?>
    <p>No messages yet.</p>
  <?php endif; ?>
  <?php foreach ($notes as $n): ?>
    <div class='note'>
      <strong><?= e($n['name']) ?></strong> <small><?= e($n['time']) ?></small>
      <p><?= nl2br(e($n['message'])) ?></p>
    </div>
  <?php endforeach; ?>
</body>
</html>