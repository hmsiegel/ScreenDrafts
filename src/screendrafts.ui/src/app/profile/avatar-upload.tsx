'use client';

import { useRef, useState } from "react";

interface AvatarUploadProps {
  currentAvatarUrl: string | null | undefined;
  displayName: string;
  accessToken: string | undefined;
  apiBase: string;
}

function Initials({ name }: { name: string }) {
  const parts = name.trim().split(/\s+/);
  const letters = parts.length >= 2
    ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
    : name.slice(0, 2).toUpperCase();
  return (
    <div className="w-[120px] h-[120px] rounded-full bg-sd-blue flex items-center justify-center">
      <span className="font-oswald font-bold text-[36px] text-white leading-none">{letters}</span>
    </div>
  );
}

export default function AvatarUpload({ currentAvatarUrl, displayName, accessToken, apiBase }: AvatarUploadProps) {
  const [avatarUrl, setAvatarUrl] = useState(currentAvatarUrl);
  const [status, setStatus] = useState<'idle' | 'uploading' | 'success' | 'error'>('idle');
  const [errorMsg, setErrorMsg] = useState('');
  const fileRef = useRef<HTMLInputElement>(null);

  async function handleUpload() {
    const file = fileRef.current?.files?.[0];
    if (!file) return;

    if (file.size > 2 * 1024 * 1024) {
      setStatus('error');
      setErrorMsg('File must be under 2MB.');
      return;
    }

    setStatus('uploading');
    setErrorMsg('');
    try {
      const form = new FormData();
      form.append('avatar', file);
      const res = await fetch(`${apiBase}/users/profile/avatar`, {
        method: 'POST',
        headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : {},
        body: form,
      });
      if (!res.ok) throw new Error(`${res.status}`);
      const data = await res.json() as { avatarUrl?: string };
      if (data.avatarUrl) setAvatarUrl(data.avatarUrl);
      setStatus('success');
    } catch (err) {
      console.error('[AvatarUpload]', err);
      setStatus('error');
      setErrorMsg('Upload failed. Please try again.');
    }
  }

  return (
    <div className="space-y-5">
      <div className="flex justify-center">
        {avatarUrl ? (
          <img
            src={avatarUrl}
            alt={displayName}
            className="w-[120px] h-[120px] rounded-full object-cover border border-sd-ink/20"
          />
        ) : (
          <Initials name={displayName} />
        )}
      </div>

      <p className="text-[12px] text-sd-ink/50 text-center">
        Profile pictures are stored and served from the ScreenDrafts CDN.
      </p>

      <div className="space-y-3">
        <input
          ref={fileRef}
          type="file"
          accept=".jpg,.jpeg,.png,.webp"
          className="block w-full text-sm text-sd-ink/60 file:mr-3 file:py-1.5 file:px-3 file:border file:border-sd-ink/30 file:bg-sd-paper file:text-sd-ink file:text-sm file:font-mono cursor-pointer"
        />
        <button
          onClick={handleUpload}
          disabled={status === 'uploading'}
          className="w-full bg-sd-red text-white font-oswald tracking-wide uppercase px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50 transition-colors"
        >
          {status === 'uploading' ? 'UPLOADING…' : 'UPLOAD AVATAR'}
        </button>
      </div>

      {status === 'success' && (
        <p className="text-[13px] text-green-700">Avatar updated successfully.</p>
      )}
      {status === 'error' && (
        <p className="text-[13px] text-sd-red">{errorMsg}</p>
      )}
    </div>
  );
}
