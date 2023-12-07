import { AdminMain } from '@/app/templates/AdminMain';
import { PnAkti } from '@/app/admin/pnakti/components/pnakti';
import { Metadata } from 'next'
import Link from 'next/link';

const title = 'P/N akti';

// Static metadata
export const metadata: Metadata = {
  title: title,
}

const AktiPage = () => {
  return (
    <AdminMain
      pageTitle={title}
      background='transparent'
      breadcrumb={[
        { title: <Link href="/admin">Sākums</Link> },
        { title: title },
      ]}
    >
      <PnAkti />
    </AdminMain>
  );
};

export default AktiPage;


