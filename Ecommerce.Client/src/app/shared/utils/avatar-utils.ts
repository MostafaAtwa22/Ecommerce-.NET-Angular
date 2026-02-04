export type AvatarGender = 'Male' | 'Female' | string | null | undefined;

const MALE_AVATAR_PATH = 'assets/users/default-male.png';
const FEMALE_AVATAR_PATH = 'assets/users/default-female.png';

export function getDefaultAvatarByGender(gender?: AvatarGender): string {
  const normalized = gender?.toString().toLowerCase();

  if (normalized === 'male') {
    return MALE_AVATAR_PATH;
  }

  if (normalized === 'female') {
    return FEMALE_AVATAR_PATH;
  }

  // Fallback if gender is not provided or unknown
  return MALE_AVATAR_PATH;
}

export function resolveUserAvatar(
  profilePicture: string | null | undefined,
  gender?: AvatarGender
): string {
  if (profilePicture) {
    return profilePicture;
  }

  return getDefaultAvatarByGender(gender);
}

