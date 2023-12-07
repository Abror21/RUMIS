import { AdminMain } from '@/app/templates/AdminMain';
import { DocumentTemplates } from '@/app/admin/document-templates/components/documents';
import { Metadata } from 'next'
import Link from 'next/link';
import Permission from '@/app/components/Permission';

const title = 'Dokumentu šabloni';

// Static metadata
export const metadata: Metadata = {
  title: title,
}

const DocumentTemplatesPage = () => {
  return (
    <AdminMain
      pageTitle={title}
      background="#f5f5f5"
      breadcrumb={[
        { title: <Link href="/admin">Sākums</Link> },
        { title: title },
      ]}
    >
      {/* <Permission permission="document_template.view" redirectTo='/'> */}
        <DocumentTemplates />
      {/* </Permission> */}
    </AdminMain>
  );
};

export default DocumentTemplatesPage;

