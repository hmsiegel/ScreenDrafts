"use client";

import { useState, useRef } from "react";
import { DRAFT_IMAGE_FALLBACKS, draftImageCandidates } from "@/lib/cdn";
import FallbackImage from "@/components/features/drafts/fallback-image";

const BTN_SECONDARY =
  "border border-sd-ink/20 text-sd-ink font-sans text-sm px-4 py-2 hover:bg-sd-ink/5 disabled:opacity-50 transition-colors rounded";
const SECTION_HEADING =
  "font-oswald font-bold text-[18px] tracking-wide uppercase text-sd-ink mb-4 pb-2 border-b border-sd-ink/10";
const IMAGE_CLASS = "w-full max-w-sm object-cover border border-sd-ink/10 rounded";

/**
 * Turns a failed upload response into a sentence for the user.
 * The raw body goes to the console for debugging; it never reaches the UI.
 */
async function describeUploadFailure(response: Response): Promise<string> {
  let detail: string | undefined;
  try {
    const body = await response.json();
    console.error("Draft image upload failed", response.status, body);
    if (typeof body?.detail === "string" && body.detail.trim() !== "") {
      detail = body.detail;
    }
  } catch {
    console.error("Draft image upload failed", response.status, response.statusText);
  }

  switch (response.status) {
    case 400:
    case 415:
    case 422:
      return detail ?? "That file couldn't be used. Upload a JPEG, PNG, or WebP image.";
    case 401:
      return "Your session has expired. Sign in again, then retry the upload.";
    case 403:
      return "You don't have permission to change this draft's image.";
    case 404:
      return "This draft no longer exists.";
    case 413:
      return "That file is too large. Choose a smaller image.";
    default:
      return response.status >= 500
        ? "The server couldn't save the image. Try again in a moment."
        : `Upload failed (${response.status}). Try again.`;
  }
}

interface Props {
  draftPublicId: string;
  currentImagePath: string | null | undefined;
  accessToken: string;
  apiBase: string;
  onUploaded?: (newPath: string) => void;
}

export default function DraftImageUpload({
  draftPublicId,
  currentImagePath,
  accessToken,
  apiBase,
  onUploaded,
}: Props) {
  const [preview, setPreview] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successPath, setSuccessPath] = useState<string | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const displayPath = successPath ?? currentImagePath;
  const imageSources = [...draftImageCandidates(draftPublicId, displayPath), ...DRAFT_IMAGE_FALLBACKS];

  function handleFileChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setError(null);
    setPreview(URL.createObjectURL(file));
  }

  async function handleUpload() {
    const file = fileRef.current?.files?.[0];
    if (!file) return;

    setUploading(true);
    setError(null);

    try {
      const formData = new FormData();
      formData.append("file", file);

      let response: Response;
      try {
        response = await fetch(
          `${apiBase}/drafts/${encodeURIComponent(draftPublicId)}/image`,
          {
            method: "POST",
            headers: { Authorization: `Bearer ${accessToken}` },
            body: formData,
          }
        );
      } catch (networkError) {
        console.error("Draft image upload could not reach the API", networkError);
        throw new Error("Couldn't reach the server. Check your connection and try again.");
      }

      if (!response.ok) {
        throw new Error(await describeUploadFailure(response));
      }

      const data = (await response.json()) as { imagePath: string };
      setSuccessPath(data.imagePath);
      setPreview(null);
      if (fileRef.current) fileRef.current.value = "";
      onUploaded?.(data.imagePath);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Upload failed.");
    } finally {
      setUploading(false);
    }
  }

  function handleCancel() {
    setPreview(null);
    setError(null);
    if (fileRef.current) fileRef.current.value = "";
  }

  return (
    <section>
      <h2 className={SECTION_HEADING}>Episode Image</h2>

      {/* Current / preview image */}
      <div className="mb-4">
        {preview ? (
          <div className="relative">
            <img
              src={preview}
              alt="Preview"
              className={IMAGE_CLASS}
            />
            <span className="absolute top-2 left-2 bg-sd-blue text-white text-[10px] font-mono px-2 py-0.5 rounded">
              PREVIEW
            </span>
          </div>
        ) : (
          <FallbackImage
            sources={imageSources}
            alt="Current episode image"
            className={IMAGE_CLASS}
            fallback={
              <div className="w-full max-w-sm h-40 border-2 border-dashed border-sd-ink/20 rounded flex items-center justify-center">
                <span className="text-sm font-mono text-sd-ink/40">No image uploaded</span>
              </div>
            }
          />
        )}
      </div>

      {/* File input */}
      <div className="flex items-center gap-3 flex-wrap">
        <input
          ref={fileRef}
          type="file"
          accept="image/jpeg,image/png,image/webp"
          onChange={handleFileChange}
          className="text-sm text-sd-ink/70 file:mr-3 file:py-1.5 file:px-3 file:border file:border-sd-ink/20 file:rounded file:text-sm file:font-mono file:bg-white file:text-sd-ink hover:file:bg-sd-ink/5 file:cursor-pointer"
        />
        {preview && (
          <>
            <button
              type="button"
              onClick={handleUpload}
              disabled={uploading}
              className="bg-sd-red text-white font-oswald font-medium tracking-wide uppercase px-4 py-2 hover:bg-sd-red/90 disabled:opacity-50 transition-colors text-sm"
            >
              {uploading ? "Uploading…" : "Upload"}
            </button>
            <button
              type="button"
              onClick={handleCancel}
              disabled={uploading}
              className={BTN_SECONDARY}
            >
              Cancel
            </button>
          </>
        )}
      </div>

      <p className="mt-2 text-[11px] font-mono text-sd-ink/40">
        JPEG, PNG, or WebP. Recommended: square, at least 800×800px.
      </p>

      {error && (
        <p className="mt-2 text-sm text-red-600 font-mono">{error}</p>
      )}
    </section>
  );
}